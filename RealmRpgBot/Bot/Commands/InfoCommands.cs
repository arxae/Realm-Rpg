namespace RealmRpgBot.Bot.Commands
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;
	using Raven.Client.Documents;

	[Group("info")]
	public class InfoCommands : RpgCommandBase
	{
		[Command("races"), Description("List available races")]
		public async Task ListRaces(CommandContext c)
		{
			using (var s = Db.DocStore.OpenAsyncSession())
			{
				var raceList = await s.Query<Models.Race>("Races/All").ToListAsync();

				var sb = new System.Text.StringBuilder();
				foreach (var r in raceList) sb.AppendLine($"- {r.DisplayName}");

				var embed = new DiscordEmbedBuilder()
					.WithTitle("Available races:")
					.WithDescription(sb.ToString())
					.WithFooter("TODO: .info race human")
					.Build();

				await c.RespondAsync(embed: embed);
			}

			await c.ConfirmMessage();
		}
	}
}
