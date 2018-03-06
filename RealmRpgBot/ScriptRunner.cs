﻿namespace RealmRpgBot
{
	using System;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using MoonSharp.Interpreter;

	using Models.Character;

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
			_lua.Globals["Rest"] = (Action)(async () => await PlaceHolder());

			// Register types
			UserData.RegisterType<Player>();
			UserData.RegisterType<AttributeBlock>();
			UserData.RegisterType<TrainedSkill>();

			_ctx = c;
			_botReply = botReply;

			_log.Debug("ScriptRunner initialized for {ursname} ({userid})", c.GetFullUserName(), c.User.Id);
		}

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

		async Task DiscordReplyAsync(string replyMsg) => await _ctx.RespondAsync(ParseMessageMention(_ctx, replyMsg));
		async Task DiscordSetMessage(string newMsg) => await _botReply?.ModifyAsync(ParseMessageMention(_ctx, newMsg), null);
		async Task DiscordDeleteResponse() => await _botReply?.DeleteAsync();

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

		async Task HealPlayerToFull()
		{
			await Task.Run(() =>
			{
				_sourcePlayer.HpCurrent = _sourcePlayer.HpMax;
			});
		}

		async Task PlaceHolder()
		{
			await Task.Run(() =>
			{
				_log.Warning("Placeholder Called");
			});
		}

		string ParseMessageMention(CommandContext c, string msg)
		{
			return msg.ReplaceCaseInsensitive("@mention", c.User.Mention);
		}
	}
}
