namespace RealmRpgBot
{
	using System;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using MoonSharp.Interpreter;

	using Models.Character;

	/// <summary>
	/// Executes Lua scripts
	/// </summary>
	public class ScriptRunner
	{
		Script _lua;
		Serilog.ILogger _log;

		CommandContext _ctx;
		DiscordMessage _botReply;
		Player _sourcePlayer;

		public ScriptRunner(CommandContext c, DiscordMessage botReply)
		{
			_log = Serilog.Log.ForContext<ScriptRunner>();
			_lua = new Script
			{
				Options = { DebugPrint = s => _log.Debug(s) }
			};

			// Register globals
			_lua.Globals["Reply"] = (Action<string>)(async replyMsg => await DiscordReplyAsync(replyMsg));
			_lua.Globals["SetMessage"] = (Action<string>)(async newMsg => await DiscordSetMessage(newMsg));
			_lua.Globals["DeleteResponse"] = (Action)(async () => await DiscordDeleteResponse());
			_lua.Globals["HealPlayer"] = (Action<int>)(async amt => await HealPlayer(amt));
			_lua.Globals["FullHealPlayer"] = (Action)(async () => await HealPlayerToFull());

			// Register types
			UserData.RegisterType<Player>();
			UserData.RegisterType<AttributeBlock>();
			UserData.RegisterType<TrainedSkill>();

			_ctx = c;
			_botReply = botReply;

			_log.Debug("ScriptRunner initialized for {ursname} ({userid})", c.GetFullUserName(), c.User.Id);
		}

		/// <summary>
		/// Runs a script
		/// </summary>
		/// <param name="script">Lua string</param>
		/// <returns></returns>
		public async Task PerformScriptAsync(string script)
		{
			using (var session = Db.DocStore.OpenSession())
			{
				_sourcePlayer = session.Load<Player>(_ctx.User.Id.ToString());

				_lua.Globals["Player"] = _sourcePlayer;

				await _lua.DoStringAsync(script);

				if (session.Advanced.HasChanged(_sourcePlayer))
				{
					session.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Adds a reply in discord. This method is mapped to Reply("msg") in Lua
		/// </summary>
		/// <param name="replyMsg">Message to reply</param>
		/// <returns></returns>
		async Task DiscordReplyAsync(string replyMsg) => await _ctx.RespondAsync(ParseMessageMention(_ctx, replyMsg));

		/// <summary>
		/// Changes the bot message. This method is mapped to SetMessage("msg") in Lua
		/// </summary>
		/// <param name="newMsg">What to change the message to</param>
		/// <returns></returns>
		async Task DiscordSetMessage(string newMsg) => await _botReply?.ModifyAsync(ParseMessageMention(_ctx, newMsg), null);

		/// <summary>
		/// Deletes the bot message. This method is mapped to DeleteResponse() in Lua
		/// </summary>
		/// <returns></returns>
		async Task DiscordDeleteResponse() => await _botReply?.DeleteAsync();

		/// <summary>
		/// Heals the current player. This method is mapped to HealPlayer(1) in Lua
		/// </summary>
		/// <param name="amount">Amount of HP to heal</param>
		/// <returns></returns>
		async Task HealPlayer(int amount)
		{
			await Task.Run(() =>
			{
				_sourcePlayer.HpCurrent += amount;
				if (_sourcePlayer.HpCurrent > _sourcePlayer.HpMax)
				{
					_sourcePlayer.HpCurrent = _sourcePlayer.HpMax;
				}
			});
		}

		/// <summary>
		/// Fully heals the current player. This method is mapped to FullHealPlayer() in Lua
		/// </summary>
		/// <returns></returns>
		async Task HealPlayerToFull()
		{
			await Task.Run(() =>
			{
				_sourcePlayer.HpCurrent = _sourcePlayer.HpMax;
			});
		}

		/// <summary>
		/// Replace the @mention string to the actual mention of the command issuer
		/// </summary>
		/// <param name="c"></param>
		/// <param name="msg">Message to check</param>
		/// <returns></returns>
		string ParseMessageMention(CommandContext c, string msg)
		{
			return msg.ReplaceCaseInsensitive("@mention", c.User.Mention);
		}
	}
}
