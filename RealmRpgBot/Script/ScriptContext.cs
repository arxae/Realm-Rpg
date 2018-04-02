namespace RealmRpgBot.Script
{
	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using DSharpPlus.Interactivity;

	using Models.Character;

	public class ScriptContext
	{
		public Player Player { get; set; }
		public CommandContext CmdContext { get; set; }
		public DiscordMessage BotMessage { get; set; }
		public DiscordMessage LastMessage { get; set; }
		public InteractivityExtension Interact { get; set; }
		public string PlayerMention => $"<@{Player.Id}>";
		public string LastReactionName { get; set; }

		public ScriptContext(Player p, CommandContext c, DiscordMessage m)
		{
			Player = p;
			CmdContext = c;
			BotMessage = m;
			Interact = CmdContext.Client.GetInteractivity();
		}

		public bool HasPlayer => Player != null;

		public void Log(string message) => Serilog.Log.ForContext<ScriptContext>().Information(message);
		public void Reply(string message) => LastMessage = CmdContext.RespondAsync(ParseMessageMention(message)).ConfigureAwait(false).GetAwaiter().GetResult();
		public void SetMessage(string message) => BotMessage?.ModifyAsync(ParseMessageMention(message), null);
		public void DeleteMessage() => BotMessage?.DeleteAsync("Scripted remove");
		public void HealPlayer(int amount) => Player?.HealHpAsync(amount);
		public void FullHealPlayer() => Player?.HealHpAsync(Player.HpMax);
		public void RestoreMana(int amount) => Player?.RestoreMpAsync(amount);
		public void FullRestoreMana() => Player?.RestoreMpAsync(Player.ManaMax);

		public void AddReaction(string emojiName)
		{
			string qualName = emojiName.StartsWith(":") == false || emojiName.EndsWith(":") == false
				? $":{emojiName.Replace(":", "")}:"
				: emojiName;

			LastMessage.CreateReactionAsync(DiscordEmoji.FromName(CmdContext.Client, qualName))
				.GetAwaiter()
				.GetResult();
		}

		public string GetReactionResponse(int timeoutS)
		{
			var response =
				Interact.WaitForMessageReactionAsync(LastMessage, CmdContext.User, System.TimeSpan.FromSeconds(timeoutS))
					.GetAwaiter()
					.GetResult();
			if (response == null) return string.Empty;

			return response.Emoji.GetDiscordName().ToLower();
		}

		private string ParseMessageMention(string message) => message.ReplaceCaseInsensitive("@mention", CmdContext.User.Mention);
	}
}