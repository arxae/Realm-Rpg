﻿namespace RealmRpgBot.Bot.Commands
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;
	using Raven.Client.Documents;

	using Combat;
	using Models.Character;
	using Models.Encounters;
	using Models.Map;

	/// <summary>
	/// Various actions player can perform
	/// </summary>
	[RequireRoles(RoleCheckMode.All, "Realm Player", "Realm Admin")]
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
					await c.RespondAsync($"{c.User.Mention}, {Constants.MSG_NOT_REGISTERED}");
					await c.RejectMessage();

					return;
				}

				var location = await session.LoadAsync<Location>(player.CurrentLocation);
				var locEmbed = location.GetLocationEmbed();

				await c.RespondAsync(c.User.Mention, embed: locEmbed);
				await c.ConfirmMessage();
			}
		}

		/// <summary>
		/// Travel to a location
		/// </summary>
		/// <param name="c"></param>
		/// <param name="destinationName">The name of the destination</param>
		/// <returns></returns>
		[Command("travel"), Description("Travel to a specific location that is linked to the current location")]
		public async Task Travel(CommandContext c,
			[Description("Name of the destination"), RemainingText]
			string destinationName)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Location>(loc => loc.Id)
					.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync($"{c.User.Mention}, {Constants.MSG_NOT_REGISTERED}");
					await c.RejectMessage();

					return;
				}

				if (player.IsIdle == false)
				{
					await c.RespondAsync($"{c.User.Mention}, {Constants.MSG_PLAYER_NOT_IDLE}: {player.CurrentActionDisplay}");
					await c.RejectMessage();
					return;
				}

				var location = await session.LoadAsync<Location>(player.CurrentLocation);
				if (location.LocationConnections == null || location.LocationConnections.Count == 0)
				{
					await c.RespondAsync(Constants.MSG_NO_EXITS);
					await c.RejectMessage();
					return;
				}

				if (location.LocationConnections.Contains(destinationName, System.StringComparer.OrdinalIgnoreCase) == false)
				{
					await c.RespondAsync(Constants.MSG_INVALID_CONNECTION);
					await c.RejectMessage();
					return;
				}

				var dest = await session.Query<Location>().FirstOrDefaultAsync(tl => tl.DisplayName == destinationName);

				if (dest != null)
				{
					player.CurrentLocation = dest.Id;
					await session.SaveChangesAsync();

					await c.RespondAsync($"{c.User.Mention} has arrived in {dest.DisplayName}");
					await c.ConfirmMessage();
				}
				else
				{
					await c.RespondAsync($"{destinationName} is not a valid destination");
					await c.RejectMessage();
				}
			}
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
					await c.RespondAsync($"{c.User.Mention}, {Constants.MSG_NOT_REGISTERED}");
					await c.RejectMessage();

					return;
				}

				if (player.IsIdle == false)
				{
					await c.RespondAsync($"{c.User.Mention}, {Constants.MSG_PLAYER_NOT_IDLE}: {player.CurrentActionDisplay}");
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
		public async Task TrainAttributes(CommandContext c,
			[Description("Full or shorthand name of the attribute to train")]
			string attributeName)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync(Constants.MSG_NOT_REGISTERED);
					await c.RejectMessage();
					return;
				}

				if (player.AttributePoints < 1)
				{
					await c.RespondAsync(Constants.MSG_NOT_ENOUGH_ATTRIB_PTS);
					await c.RejectMessage();
					return;
				}

				string trainingMessage;
				switch (attributeName.ToLower())
				{
					case "str":
					case "strength":
						player.Attributes.Strength++;
						player.AttributePoints--;
						trainingMessage = " has been lifting, increasing his strength";
						break;

					case "agi":
					case "agility":
						player.Attributes.Agility++;
						player.AttributePoints--;
						trainingMessage = " has been doing flips and shit";
						break;

					case "sta":
					case "stamina":
						player.Attributes.Stamina++;
						player.AttributePoints--;
						trainingMessage = " has been roughed up in a place we do not talk about";
						break;

					case "int":
					case "intelligence":
						player.Attributes.Intelligence++;
						player.AttributePoints--;
						trainingMessage = " has been sleeping. With some books.";
						break;

					case "wis":
					case "wisdom":
						player.Attributes.Wisdom++;
						player.AttributePoints--;
						trainingMessage = " has been sitting on his knees in church. Praying for sure";
						break;

					default:
						await c.RespondAsync(
							$"Attribute *{attributeName}* is invalid. Valid options are: STR/Strength, AGI/Agility, STA/Stamina, INT/Intelligence, WIS/Wisdom");
						await c.RejectMessage();
						return;
				}

				if (session.Advanced.HasChanged(player))
				{
					await session.SaveChangesAsync();
					await c.RespondAsync($"{c.User.Mention} {trainingMessage}");
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
					await c.RespondAsync(Constants.MSG_NOT_REGISTERED);
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
					// TODO: Generic events
					await c.RespondAsync("Nothing to do here");
					await c.ConfirmMessage();
					return;
				}

				var encounterId = location.Encounters?.GetRandomEntry();
				var encounter = await session.LoadAsync<Encounter>(encounterId);

				if (encounter.EncounterType == Encounter.EncounterTypes.Enemy)
				{
					var template = await session.LoadAsync<EncounterTemplate>(encounter.TemplateId);
					var enemy = new Enemy(encounter.Name, template, player.Level);

					var encounterEmbed = new DiscordEmbedBuilder()
						.WithTitle($"Encounter with {enemy.Name}");

					var body = new System.Text.StringBuilder();
					body.AppendLine($"{c.User.Mention} has encountered a lvl{enemy.Level} {enemy.Name} with {enemy.HpCurrent}hp.");

					var combat = new AutoBattle(player, enemy, c);
					await combat.StartCombatAsync();

					var result = await combat.GetCombatResultAsync();

					string xpMsg = string.Empty;
					switch (result.Outcome)
					{
						case CombatOutcome.Attacker:
							{
								var xpGain = enemy.Level; // TODO: propper xp calculation
								await player.AddXpAsync(xpGain, c);
								xpMsg = $"Gained {xpGain}xp";
								break;
							}
						case CombatOutcome.Defender:
							{
								var xpLost = await player.SetFaintedAsync();
								xpMsg = $"Lost {xpLost}xp.";
								break;
							}
						case CombatOutcome.Tie:
							{
								var xpGain = enemy.Level; // TODO: propper xp calculation
								await player.AddXpAsync(xpGain, c);
								var xpLost = await player.SetFaintedAsync();
								xpMsg = $"Gained {xpGain}xp, but lost {xpLost}xp.";
								break;
							}
					}

					encounterEmbed.WithFooter(xpMsg);

					body.AppendLine();
					body.AppendLine("*Last lines of Combat Log*");
					body.AppendLine("*...*");

					foreach (var line in combat.CombatLog.Skip(System.Math.Max(0, combat.CombatLog.Count - 3)))
					{
						body.AppendLine(line);
					}

					body.AppendLine();
					body.AppendLine(result.Message);

					encounterEmbed.WithDescription(body.ToString());

					await player.SetActionAsync(Constants.ACTION_REST, "Recovering from combat", System.TimeSpan.FromMinutes(1));

					await c.RespondAsync(embed: encounterEmbed.Build());
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