namespace RealmRpgBot.Script
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;

	using Models.Character;

	public class ScriptContext
	{
		public Player Player { get; set; }
		public CommandContext CmdContext { get; set; }
		public DiscordMessage BotMessage { get; set; }

		public ScriptContext(Player p, CommandContext c, DiscordMessage m)
		{
			Player = p;
			CmdContext = c;
			BotMessage = m;
		}

		public bool HasPlayer => Player != null;

		public void Log(string message)
		{
			Serilog.Log.ForContext<ScriptContext>().Information(message);
		}

		public void Reply(string message)
		{
			CmdContext.RespondAsync(ParseMessageMention(message));
		}

		public void SetMessage(string message)
		{
			BotMessage?.ModifyAsync(ParseMessageMention(message), null);
		}

		public void DeleteMessage()
		{
			BotMessage?.DeleteAsync("Scripted remove");
		}

		public void HealPlayer(int amount)
		{
			Player?.HealHpAsync(amount);
		}

		public void FullHealPlayer()
		{
			Player?.HealHpAsync(Player.HpMax);
		}

		private string ParseMessageMention(string message) => message.ReplaceCaseInsensitive("@mention", CmdContext.User.Mention);
	}
}