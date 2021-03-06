﻿using RealmRpgBot.Models.Inventory;

namespace RealmRpgBot.Bot.Commands
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;
	using Raven.Client.Documents;

	using Models.Character;
	using Models.Map;

	[Group("dev"), Description("Game administration commands")]
	public class DeveloperCommands : RpgCommandBase
	{
		[Command("addxp"), Description("Add specified amount of xp to a player")]
		public async Task GiveXp(CommandContext c,
			[Description("User mention who should receive the xp")] DiscordUser mention,
			[Description("The amount of xp the user will receive")] int xpAmount)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(mention.Id.ToString());
				if (player == null)
				{
					await c.RespondAsync(Realm.GetMessage("user_not_registered"));
					await c.RejectMessage();

					return;
				}

				await player.AddXpAsync(xpAmount, c);
				await session.SaveChangesAsync();

				await c.ConfirmMessage();
			}
		}

		[Command("levelup"), Description("Levels up specific player")]
		public async Task LevelUp(CommandContext c,
			[Description("User mention who should get the levelup")] DiscordUser mention)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(mention.Id.ToString());
				if (player == null)
				{
					await c.RejectMessage(Realm.GetMessage("user_not_registered"));
					return;
				}

				var xpNeeded = player.XpNext - player.XpCurrent;
				await player.AddXpAsync(xpNeeded, c);
				await session.SaveChangesAsync();
			}

			await c.ConfirmMessage();
		}

		[Command("addlevels"), Description("Add specified amount of levels to a character")]
		public async Task AddLevels(CommandContext c,
			[Description("")] DiscordUser mention,
			[Description("")] int levels)
		{
			if (levels > 10)
			{
				await c.RejectMessage("Cannot add more then 10 levels at the same time");
				return;
			}

			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(mention.Id.ToString());
				if (player == null)
				{
					await c.RejectMessage(Realm.GetMessage("user_not_registered"));
					return;
				}

				for (int i = 0; i < levels; i++)
				{
					var xpNeeded = player.XpNext - player.XpCurrent;
					await player.AddXpAsync(xpNeeded, c);
				}

				await session.SaveChangesAsync();
			}

			await c.ConfirmMessage();
		}

		[Command("teleport"), Aliases("tp")]
		public async Task Teleport(CommandContext c,
			[Description("User to teleport")] DiscordUser mention,
			[Description("Location to teleport to"), RemainingText] string locationName)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Location>(loc => loc.DisplayName)
					.LoadAsync<Player>(mention.Id.ToString());
				if (player == null)
				{
					await c.RespondAsync(Realm.GetMessage("user_not_registered"));
					await c.RejectMessage();

					return;
				}

				var location = await session.Query<Location>().FirstOrDefaultAsync(l =>
					l.DisplayName.Equals(locationName, System.StringComparison.OrdinalIgnoreCase));
				if (location == null)
				{
					await c.RespondAsync("Invalid location");
					await c.RejectMessage();
					return;
				}

				player.CurrentLocation = location.Id;
				await session.SaveChangesAsync();

				var user = await c.Guild.GetMemberAsync(mention.Id);
				await user.SendMessageAsync($"[REALM] You have been teleported to {location.DisplayName}");
			}

			await c.ConfirmMessage();
		}

		[Command("clearcache"), Aliases("cc"), Description("Clear caches")]
		public async Task ClearSettingsCache(CommandContext c)
		{
			Realm.ClearSettingsCache();
			await c.ConfirmMessage();
		}

		[Command("unlockexits"), Description("Unlocks all locations for a player on it's location)")]
		public async Task UnlockExits(CommandContext c,
			[Description("")] DiscordUser mention,
			[Description("Also add hidden exits to user")] bool includeHidden = false)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Location>(lc => lc.Id)
					.LoadAsync<Player>(mention.Id.ToString());

				if (player == null)
				{
					await c.RejectMessage("That user is not part of the realm");
					return;
				}

				var location = await session.LoadAsync<Location>(player.CurrentLocation);
				int unlocks = 0;
				int hiddenUnlocks = 0;
				// Regular Exits
				if (player.LocationExploreCounts.ContainsKey(player.CurrentLocation) == false)
				{
					player.LocationExploreCounts.Add(player.CurrentLocation, location.ExploresNeeded);
					unlocks += location.LocationConnections.Count;
				}
				else
				{
					if (player.LocationExploreCounts[player.CurrentLocation] < location.ExploresNeeded)
					{
						player.LocationExploreCounts[player.CurrentLocation] = location.ExploresNeeded;
						unlocks += location.LocationConnections.Count;
					}
				}

				// Hidden Exits
				if (includeHidden)
				{
					if (location.HiddenLocationConnections.Count > 0)
					{
						foreach (var l in location.HiddenLocationConnections.Values)
						{
							if (player.FoundHiddenLocations.Contains(l)) continue;
							player.FoundHiddenLocations.Add(l);
							unlocks++;
							hiddenUnlocks++;
						}
					}
				}

				var m = await c.Guild.GetMemberAsync(mention.Id);
				if (unlocks + hiddenUnlocks > 0)
				{
					string message = $"[ADMIN] {unlocks} exits have been unlocked.";
					if (hiddenUnlocks > 0)
					{
						message += $" Including {hiddenUnlocks}";
					}

					await m.SendMessageAsync(message);
				}
				else
				{
					await m.SendMessageAsync("[ADMIN] No additional exits have been unlocked for this location");
				}

				if (session.Advanced.HasChanges)
				{
					await session.SaveChangesAsync();
				}
			}

			await c.ConfirmMessage();
		}

		[Command("giveitem"), Description("Give a player an item")]
		public async Task GiveItem(CommandContext c,
			[Description("User who will receive the item")] DiscordUser mention,
			[Description("The itemid as it's referenced in the database")] string itemId,
			[Description("Amount of the item to give. If ommited, defaults to 1")] int amount = 1)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Item>(itm => itm.Id)
					.LoadAsync<Player>(mention.Id.ToString());

				if (player == null)
				{
					await c.RejectMessage(Realm.GetMessage("user_not_registered"));
					return;
				}

				var item = await session.LoadAsync<Item>(itemId);
				if (item == null)
				{
					await c.RejectMessage("No item with that ID exists");
					return;
				}

				player.AddItemToInventory(item.Id, amount);

				if (session.Advanced.HasChanges)
				{
					await session.SaveChangesAsync();
				}
			}

			await c.ConfirmMessage();
		}
	}
}
