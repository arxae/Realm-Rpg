﻿namespace RealmRpgBot.Bot.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;
	using DSharpPlus.Interactivity;
	using Raven.Client.Documents;

	using Models.Character;
	using Models.Encounters;
	using Models.Map;

	/// <summary>
	/// Various actions player can perform
	/// </summary>
	[RequireRoles(RoleCheckMode.Any, Constants.ROLE_PLAYER)]
	public class GeneralActionCommands : RpgCommandBase
	{
		[Command("ping"), Aliases("p"), Description("Check bot responsiveness, replies with latency")]
		public async Task Ping(CommandContext c)
		{
			await c.RespondAsync($"Pong! ({c.Client.Ping}ms)");
			await c.ConfirmMessage();
		}

		/// <summary>
		/// Get location information
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		[Command("look"), Description("Survey your surroundings")]
		public async Task Look(CommandContext c)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Location>(loc => loc.Id)
					.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync($"{c.User.Mention}, {Realm.GetMessage("not_registered")}");
					await c.RejectMessage();

					return;
				}

				int exploreCounts = -1;
				if (player.LocationExploreCounts.ContainsKey(player.CurrentLocation))
				{
					exploreCounts = player.LocationExploreCounts[player.CurrentLocation];
				}

				var location = await session.LoadAsync<Location>(player.CurrentLocation);

				var playersOnLoc = await session.Query<Player>()
					.Where(pl => pl.CurrentLocation == location.Id && pl.Id != c.User.Id.ToString())
					.Select(pl => pl.Id)
					.ToListAsync();

				var locEmbed = new DiscordEmbedBuilder(location.GetLocationEmbed(player.FoundHiddenLocations, player.PreviousLocation, exploreCounts));

				if (playersOnLoc.Count > 0)
				{
					var names = new List<string>();
					foreach (var pl in playersOnLoc)
					{
						names.Add((await c.Guild.GetMemberAsync(ulong.Parse(pl))).DisplayName);
					}

					locEmbed.AddField("Also Here", string.Join(", ", names));
				}

				await c.RespondAsync(c.User.Mention, embed: locEmbed.Build());
				await c.ConfirmMessage();
			}
		}

		/// <summary>
		/// Travel to a location
		/// </summary>
		/// <param name="c"></param>
		/// <param name="destinations">The name of the destination. Can be split with ; to pass multiple locations.
		/// Will stop at the final destination, or the last correct destination (in case of a incorrect destination name)</param>
		/// <returns></returns>
		[Command("travel")]
		public async Task TravelMultiple(CommandContext c,
			[Description("Travel to a specific location that is linked to the current location"), RemainingText] string destinations)
		{
			var dests = destinations.Split(';');
			if (dests.Length == 0)
			{
				await c.RespondAsync($"Something went wrong while parsing the travel command (Error_DestinationLengthZero {destinations})");
				await c.RejectMessage();
				return;
			}

			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Location>(l => l.Id)
					.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync($"{c.User.Mention}, {Realm.GetMessage("not_registered")}");
					await c.RejectMessage();

					return;
				}

				if (player.IsIdle == false)
				{
					await c.RespondAsync(string.Format(Realm.GetMessage("player_not_idle"), c.User.Mention, player.CurrentActionDisplay));
					await c.RejectMessage();
					return;
				}

				Serilog.Log.ForContext<GeneralActionCommands>().Debug("({u}) Checking path {p}", c.GetFullUserName(), dests.ExtendToString());

				var loc = await session.LoadAsync<Location>(player.CurrentLocation);
				bool arrivedWithIssue = false;
				string previousLocation = player.CurrentLocation;
				for (int i = 0; i < dests.Length; i++)
				{
					var d = dests[i];

					if (loc.LocationConnections.Keys.Contains(d, System.StringComparer.OrdinalIgnoreCase))
					{
						previousLocation = loc.Id;
						loc = await session.Query<Location>().FirstOrDefaultAsync(tl => tl.DisplayName == d);
						Serilog.Log.ForContext<GeneralActionCommands>().Debug("({usr}) {x}/{y} found {a}", c.GetFullUserName(), i + 1, dests.Length, loc.Id);
					}
					else if (loc.HiddenLocationConnections.Keys.Contains(d, System.StringComparer.OrdinalIgnoreCase))
					{
						// check if the player has the point
						var hiddenLoc = loc.HiddenLocationConnections[loc.HiddenLocationConnections
							.FirstOrDefault(l => l.Key.Equals(d, System.StringComparison.OrdinalIgnoreCase)).Key];
						if (player.FoundHiddenLocations.Contains(hiddenLoc))
						{
							previousLocation = loc.Id;
							loc = await session.Query<Location>().FirstOrDefaultAsync(tl => tl.DisplayName == d);
							Serilog.Log.ForContext<GeneralActionCommands>().Debug("({usr}) {x}/{y} found hidden {a}", c.GetFullUserName(), i + 1, dests.Length, loc.Id);
						}
						else
						{
							Serilog.Log.ForContext<GeneralActionCommands>().Debug("({usr}) {x}/{y} not found {a}. Stopping", c.GetFullUserName(), i + 1, dests.Length, dests[i]);
							break;
						}
					}
					else
					{
						Serilog.Log.ForContext<GeneralActionCommands>().Debug("({usr}) {x}/{y} not found {a}. Stopping", c.GetFullUserName(), i + 1, dests.Length, dests[i]);
						arrivedWithIssue = true;
						break;
					}
				}

				Serilog.Log.ForContext<GeneralActionCommands>().Debug("({usr}) End destination: {a}", c.GetFullUserName(), loc.DisplayName);

				if (player.CurrentLocation == loc.Id)
				{
					await c.RespondAsync($"{c.User.Mention} couldn't find that exit");
				}
				else if (arrivedWithIssue)
				{
					await c.RespondAsync($"{c.User.Mention} got lost, but eventually found {loc.DisplayName}");
				}
				else
				{
					await c.RespondAsync($"{c.User.Mention} arrived at {loc.DisplayName}");
				}

				player.PreviousLocation = previousLocation;
				player.CurrentLocation = loc.Id;

				await session.SaveChangesAsync();
			}

			await c.ConfirmMessage();
		}

		[Command("enter"), Description("Enter a building")]
		public async Task EnterBuilding(CommandContext c,
			[Description("Name of the building"), RemainingText]
			string buildingName)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Location>(loc => loc.Id)
					.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync($"{c.User.Mention}, {Realm.GetMessage("not_registered")}");
					await c.RejectMessage();

					return;
				}

				if (player.IsIdle == false)
				{
					await c.RespondAsync(string.Format(Realm.GetMessage("player_not_idle"), c.User.Mention, player.CurrentActionDisplay));
					await c.RejectMessage();
					return;
				}

				var location = await session.LoadAsync<Location>(player.CurrentLocation);
				var building =
					location.Buildings.FirstOrDefault(b => b.Name.Equals(buildingName, System.StringComparison.OrdinalIgnoreCase));

				if (building == null)
				{
					await c.RespondAsync($"{c.User.Mention}. No building called {buildingName} is located in {location.DisplayName}");
					await c.RejectMessage();

					return;
				}

				var bType = Realm.GetBuildingImplementation(building.BuildingImpl);
				if (bType == null)
				{
					await c.RespondAsync("An error occured. Contact one of the admins and (Error_BuildingImplNotFound)");
					await c.RejectMessage();

					return;
				}

				var bImpl = (Buildings.IBuilding)System.Activator.CreateInstance(bType);
				await bImpl.EnterBuilding(c, building);

				await c.ConfirmMessage();
			}
		}

		[Command("status"), Description("Get character status")]
		public async Task GetCharacterStatus(CommandContext c)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Race>(r => r.Id)
					.Include<CharacterClass>(cc => cc.Id)
					.LoadAsync<Player>(c.User.Id.ToString());
				var race = await session.LoadAsync<Race>(player.Race);
				var cls = await session.LoadAsync<CharacterClass>(player.Class);

				var hpPerc = (int)System.Math.Round(((double)player.HpCurrent / player.HpMax) * 100);
				var xpPerc = (int)System.Math.Round(((double)player.XpCurrent / player.XpNext) * 100);

				var hpBar = new System.Text.StringBuilder();
				hpBar.Append(string.Concat(Enumerable.Repeat("|", hpPerc / 10)));
				hpBar.Append(string.Concat(Enumerable.Repeat("-", 10 - (hpPerc / 10))));

				var xpBar = new System.Text.StringBuilder();
				xpBar.Append(string.Concat(Enumerable.Repeat("|", xpPerc / 10)));
				xpBar.Append(string.Concat(Enumerable.Repeat("-", 10 - (xpPerc / 10))));

				var description = new System.Text.StringBuilder();
				description.AppendLine("**Stats:**");
				description.AppendLine($"STR: {player.Attributes.Strength}");
				description.AppendLine($"AGI: {player.Attributes.Agility}");
				description.AppendLine($"STA: {player.Attributes.Stamina}");
				description.AppendLine($"INT: {player.Attributes.Intelligence}");
				description.AppendLine($"WIS: {player.Attributes.Wisdom}");

				var embed = new DiscordEmbedBuilder()
					.WithTitle($"{player.Name} the {race.DisplayName} {cls.DisplayName}")
					.AddField("Hp", $"{hpBar} ({player.HpCurrent}/{player.HpMax})")
					.AddField("Xp", $"{xpBar} ({player.XpCurrent}/{player.XpNext})")
					.AddField("Skill pts", player.SkillPoints.ToString(), true)
					.AddField("Attrib pts", player.AttributePoints.ToString(), true)
					.WithDescription(description.ToString());

				await c.Member.SendMessageAsync(embed: embed.Build());
			}

			await c.ConfirmMessage();
		}

		[Command("train"), Description("Increase attributes (with attribute points)")]
		public async Task TrainAttributes(CommandContext c)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync(Realm.GetMessage("not_registered"));
					await c.RejectMessage();
					return;
				}

				if (player.AttributePoints < 1)
				{
					await c.RespondAsync(Realm.GetMessage("not_enough_attrib_pts"));
					await c.RejectMessage();
					return;
				}

				var interact = c.Client.GetInteractivity();

				var desc = new System.Text.StringBuilder();

				desc.AppendLine($"{c.User.Mention}, what attribute do you want to train. Current level in parentheses");
				desc.AppendLine($"STR: :muscle: ({player.Attributes.Strength})");
				desc.AppendLine($"AGI: :hand_splayed: ({player.Attributes.Agility})");
				desc.AppendLine($"STA: :heart: ({player.Attributes.Stamina})");
				desc.AppendLine($"INT: :eye_in_speech_bubble: ({player.Attributes.Intelligence})");
				desc.AppendLine($"WIS: :six_pointed_star: ({player.Attributes.Wisdom})");

				var embed = new DiscordEmbedBuilder().WithDescription(desc.ToString());

				var msg = await c.RespondAsync(embed: embed);
				await msg.CreateReactionAsync(DiscordEmoji.FromName(c.Client, Constants.ATTRIB_STR)); // Strength
				await msg.CreateReactionAsync(DiscordEmoji.FromName(c.Client, Constants.ATTRIB_AGI)); // Agility
				await msg.CreateReactionAsync(DiscordEmoji.FromName(c.Client, Constants.ATTRIB_STA)); // Stamina
				await msg.CreateReactionAsync(DiscordEmoji.FromName(c.Client, Constants.ATTRIB_INT)); // Intelligence
				await msg.CreateReactionAsync(DiscordEmoji.FromName(c.Client, Constants.ATTRIB_WIS)); // Wisdom

				var response = await interact.WaitForMessageReactionAsync(msg, c.User, System.TimeSpan.FromSeconds(10));

				if (response == null)
				{
					await c.RejectMessage();
					await msg.DeleteAsync();
					return;
				}

				await msg.DeleteAllReactionsAsync();
				var responsename = response.Emoji.GetDiscordName().ToLower();

				switch (responsename)
				{
					case Constants.ATTRIB_STR:
						player.Attributes.Strength++;
						player.AttributePoints--;
						await c.RespondAsync(string.Format(Realm.GetMessage("train_str"), c.User.Mention));
						break;
					case Constants.ATTRIB_AGI:
						player.Attributes.Agility++;
						player.AttributePoints--;
						await c.RespondAsync(string.Format(Realm.GetMessage("train_agi"), c.User.Mention));
						break;
					case Constants.ATTRIB_STA:
						player.Attributes.Stamina++;
						player.AttributePoints--;
						await c.RespondAsync(string.Format(Realm.GetMessage("train_sta"), c.User.Mention));
						break;
					case Constants.ATTRIB_INT:
						player.Attributes.Intelligence++;
						player.AttributePoints--;
						await c.RespondAsync(string.Format(Realm.GetMessage("train_int"), c.User.Mention));
						break;
					case Constants.ATTRIB_WIS:
						player.Attributes.Wisdom++;
						player.AttributePoints--;
						await c.RespondAsync(string.Format(Realm.GetMessage("train_wis"), c.User.Mention));
						break;
				}

				await msg.DeleteAsync();

				if (session.Advanced.HasChanged(player))
				{
					await session.SaveChangesAsync();
				}
			}

			await c.ConfirmMessage();
		}

		[Command("take"), Description("Take an item from the current location")]
		public async Task TakeItemFromLocation(CommandContext c,
			[Description("The name of the item you want to take"), RemainingText]
			string itemName)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Location>(l => l.Id)
					.LoadAsync<Player>(c.User.Id.ToString());
				var location = await session.LoadAsync<Location>(player.CurrentLocation);

				var inv = location.LocationInventory.FirstOrDefault(i =>
					i.DisplayName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));

				if (inv == null)
				{
					await c.RespondAsync($"Could not find item {itemName} on this location");
					await c.RejectMessage();
					return;
				}

				if (inv.Amount == 0)
				{
					location.LocationInventory.Remove(inv);
					await c.RespondAsync($"Could not find item {itemName} on this location");
					await c.RejectMessage();
					return;
				}

				// Add item to player
				player.AddItemToInventory(inv.DocId, inv.Amount);

				// Remove item/amount from location
				location.LocationInventory.Remove(inv);

				await session.SaveChangesAsync();
			}

			await c.ConfirmMessage();
		}

		[Command("stopaction"), Description("Stop current action and go back to idle. Skill cooldown will still apply")]
		public async Task StopCurrentAction(CommandContext c)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync(Realm.GetMessage("not_registered"));
					await c.RejectMessage();
					return;
				}

				player.SetIdleAction();
				await session.SaveChangesAsync();

				await c.ConfirmMessage();
			}
		}

		[Command("explore"), Description("Explore current location")]
		public async Task ExploreLocation(CommandContext c)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Location>(loc => loc.Id)
					.Include<Encounter>(enc => enc.Id)
					.LoadAsync<Player>(c.User.Id.ToString());
				var location = await session.LoadAsync<Location>(player.CurrentLocation);

				if (location.Encounters == null || location.Encounters?.Count == 0)
				{
					var fluffEvents = new List<string>();
					fluffEvents.AddRange(location.FluffEvents);
					fluffEvents.AddRange(Realm.GetSetting<string[]>("generic_fluff_events"));
					await c.RespondAsync(fluffEvents.GetRandomEntry());
				}
				else
				{
					var encounterId = location.Encounters?.GetRandomEntry();
					var encounter = await session.LoadAsync<Encounter>(encounterId);

					if (encounter?.EncounterType == Encounter.EncounterTypes.Battle)
					{
						await encounter.DoBattleEncounter(session, c, player);
					}
				}

				if (player.LocationExploreCounts.ContainsKey(player.CurrentLocation) == false)
				{
					player.LocationExploreCounts.Add(player.CurrentLocation, 0);
				}

				player.LocationExploreCounts[player.CurrentLocation] += 1;

				if (location.ExploresNeeded == player.LocationExploreCounts[player.CurrentLocation])
				{
					int xp = Realm.GetSetting<int>("settings/complete_explore_xp_award");
					await player.AddXpAsync(xp);
					await c.RespondAsync($"{c.User.Mention} has discovered routes to new locations and received {xp}xp.");
				}

				if (session.Advanced.HasChanges)
				{
					await session.SaveChangesAsync();
				}

				await c.ConfirmMessage();
			}
		}

		[Command("combatlog"), Description("Shows your last combat log (if any)")]
		public async Task ShowCombatLog(CommandContext c)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var log = await session.LoadAsync<Models.CombatLog>("combatlogs/" + c.User.Id);
				if (log == null)
				{
					await c.RespondAsync("No combat log available");
					await c.ConfirmMessage();
					return;
				}

				var body = new System.Text.StringBuilder();
				body.AppendLine($"**Victor: ** {log.Winner}");
				body.AppendLine($"**Lose: ** {log.Loser}");
				body.AppendLine();
				body.AppendLine("**Log**");
				log.Lines.ForEach(l => body.AppendLine(l));

				var embed = new DiscordEmbedBuilder()
					.WithTitle("Combat Log")
					.WithDescription(body.ToString())
					.WithTimestamp(log.Timestamp);

				await c.Member.SendMessageAsync(embed: embed.Build());
			}
		}
	}
}