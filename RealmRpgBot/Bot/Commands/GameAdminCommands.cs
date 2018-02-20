﻿namespace RealmRpgBot.Bot.Commands
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;

	[Group("ga"),
		Description("Game administration commands"),
		RequireRoles(RoleCheckMode.Any, new[] { "Realm Admin" })]
	public class GameAdminCommands : RpgCommandBase
	{
		[Command("givexp"), Description("Add specified amount of xp to a player")]
		public async Task GiveXp(CommandContext c,
			[Description("User mention who should receive the xp")] DiscordUser user,
			[Description("The amount of xp the user will receive")] int xpAmount)
		{
			await c.TriggerTypingAsync();

			using (var session = Db.DocStore.OpenAsyncSession())
			{
				// Get player
				var player = await session.LoadAsync<Models.Player>(user.Id.ToString());
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
	}
}
