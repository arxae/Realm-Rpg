namespace RealmRpgBot.Bot.Commands
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;

	[Group("dev"),
		Description("Game administration commands"),
		RequireRoles(RoleCheckMode.Any, new[] { "Realm Admin" })]
	public class DeveloperCommands : RpgCommandBase
	{
		[Command("addxp"), Description("Add specified amount of xp to a player")]
		public async Task GiveXp(CommandContext c,
			[Description("User mention who should receive the xp")] DiscordUser mention,
			[Description("The amount of xp the user will receive")] int xpAmount)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				// Get player
				var player = await session.LoadAsync<Models.Player>(mention.Id.ToString());
				if (player == null)
				{
					await c.RespondAsync(Constants.MSG_USER_NOT_REGISTERED);
					await c.RejectMessage();

					return;
				}

				await player.AddXpAsync(xpAmount, c);
				await session.SaveChangesAsync();
			}
		}

		[Command("fail"), Description("This command throws an exception")]
		public async Task Fail(CommandContext c)
		{
			var d = new System.Collections.Generic.Dictionary<string, string>();
			await c.RespondAsync(d["non_existing_key"]);
			await c.RespondAsync("This command should fail");
		}
	}
}
