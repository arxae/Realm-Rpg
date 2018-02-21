﻿namespace RealmRpgBot.Bot.Commands
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;

	using RealmRpgBot.Models;

	[Group("rpg")]
	public class GameCommands : RpgCommandBase
	{
		[Command("register"), Description("Register as a new player")]
		public async Task RegisterPlayer(CommandContext c, string race)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				// Check if player Exists
				if (await session.Advanced.ExistsAsync(c.User.Id.ToString()))
				{
					await c.RespondAsync($"{c.Member.Mention} You are already registered.");
					await c.RejectMessage();

					return;
				}

				if (await session.Advanced.ExistsAsync("races/" + race) == false)
				{
					await c.RespondAsync($"{c.Member.Mention} Race \"{race}\" is not valid. You can use *.info races* to get a list of races");
					await c.RejectMessage();

					return;
				}

				await session.StoreAsync(new Player(c.User, c.Guild, "races/" + race));
				await session.SaveChangesAsync();
			}

			var role = c.Guild.Roles.FirstOrDefault(r => r.Name == "Realm Player");
			if (role == null)
			{
				await c.RespondAsync("No Realm Player role exists, Contact an administrator");
				await c.RejectMessage();

				return;
			}

			await c.Member.GrantRoleAsync(role, "Registered for Realm");

			await c.RespondAsync($"{c.User.Mention} has joined the Realm");
			await c.ConfirmMessage();
		}

		[Command("remove"), Description("Remove yourself")]
		public async Task RemovePlayer(CommandContext c)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync("You are not registered");
					await c.ConfirmMessage();
					return;
				}

				session.Delete(player);
				await session.SaveChangesAsync();
			}

			var role = c.Guild.Roles.FirstOrDefault(r => r.Name == "Realm Player");
			if (role == null)
			{
				await c.RespondAsync("No Realm Player role exists");
				await c.RejectMessage();

				return;
			}

			await c.Member.RevokeRoleAsync(role, "Deleted player registration");

			await c.RespondAsync($"{c.User.Mention} has left the Realm");
			await c.ConfirmMessage();
		}
	}
}