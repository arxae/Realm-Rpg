namespace RealmRpgBot
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using MoonSharp.Interpreter;

	using Models;

	public class ScriptRunner
	{
		static Lazy<ScriptRunner> _scriptRunnerInstance = new Lazy<ScriptRunner>(() => new ScriptRunner());
		public static ScriptRunner Get => _scriptRunnerInstance.Value;

		readonly Script lua;
		readonly Serilog.ILogger log;

		CommandContext ctx;
		DiscordMessage BotReply;
		Player SourcePlayer;

		public ScriptRunner()
		{
			log = Serilog.Log.ForContext<ScriptRunner>();
			lua = new Script();

			lua.Options.DebugPrint = s => log.Debug(s);

			lua.Globals["Reply"] = (Action<string>)(async replyMsg => await DiscordReplyAsync(replyMsg));
			lua.Globals["SetMessage"] = (Action<string>)(async newMsg => await DiscordSetMessage(newMsg));

			//lua.Globals["DeleteResponse"] = (Action<bool>)(async _ => await DiscordDeleteResponse());
			lua.Globals["DeleteResponse"] = (Action)(async () => await DiscordDeleteResponse());


			lua.Globals["HealPlayer"] = (Action<int>)(async (amt) => await HealPlayer(amt));
			lua.Globals["FullHealPlayer"] = (Action)(async () => await HealPlayerToFull());
			lua.Globals["Rest"] = (Action)(async () => await PlaceHolder());

			log.Information($"ScriptRunner initialized with {lua.Globals.Keys.Count()} global(s)");
		}

		public async Task PerformScriptAsync(CommandContext c, string script, string sourcePlayerId, DiscordMessage botReply)
		{
			ctx = c;
			BotReply = botReply;

			using (var session = Db.DocStore.OpenSession())
			{
				SourcePlayer = session.Load<Player>(sourcePlayerId);

				await lua.DoStringAsync(script);

				if (session.Advanced.HasChanged(SourcePlayer))
				{
					session.SaveChanges();
				}
			}
		}

		public async Task DiscordReplyAsync(string replyMsg) => await ctx.RespondAsync(ParseMessageMention(ctx, replyMsg));
		public async Task DiscordSetMessage(string newMsg) => await BotReply?.ModifyAsync(ParseMessageMention(ctx, newMsg), null);
		public async Task DiscordDeleteResponse() => await BotReply?.DeleteAsync();

		public async Task HealPlayer(int amount)
		{
			await Task.Run(() =>
			{
				SourcePlayer.HpCurrent += amount;
				if (SourcePlayer.HpCurrent > SourcePlayer.HpMax)
				{
					SourcePlayer.HpCurrent = SourcePlayer.HpMax;
				}
			});
		}

		public async Task HealPlayerToFull()
		{
			await Task.Run(() =>
			{
				SourcePlayer.HpCurrent = SourcePlayer.HpMax;
			});
		}

		public async Task PlaceHolder()
		{
			await Task.Run(() =>
			{
				log.Warning("Placeholder Called");
			});
		}

		string ParseMessageMention(CommandContext c, string msg)
		{
			return msg.ReplaceCaseInsensitive("@mention", c.User.Mention);
		}
	}
}
