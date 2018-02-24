namespace RealmRpgBot.Bot.Commands
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using Raven.Client.Documents;

	using Models;
	using DSharpPlus.Entities;

	/// <summary>
	/// Various actions player can perform
	/// </summary>
	public class GeneralActionCommands : RpgCommandBase
	{
		[Command("ping"), Aliases(new[] { "p" }), Description("Check bot responsiveness, replies with latency")]
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
			[Description("Name of the destination"), RemainingText] string destinationName)
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
			[Description("Name of the building"), RemainingText] string buildingName)
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
				var building = location.Buildings.FirstOrDefault(b => b.Name.Equals(buildingName, System.StringComparison.OrdinalIgnoreCase));

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
				description.AppendLine($"**Stats:**");
				description.AppendLine($"STR: {player.Attributes.Strength}");
				description.AppendLine($"AGI: {player.Attributes.Agility}");
				description.AppendLine($"STA: {player.Attributes.Stamina}");
				description.AppendLine($"INT: {player.Attributes.Intelligence}");
				description.AppendLine($"WIS: {player.Attributes.Wisdom}");

				var embed = new DiscordEmbedBuilder()
					.WithTitle($"{player.UserName} the {race.DisplayName} {cls.DisplayName}")
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
			[Description("Full or shorthand name of the attribute to train")] string attributeName)
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
						await c.RespondAsync($"Attribute *{attributeName}* is invalid. Valid options are: STR/Strength, AGI/Agility, STA/Stamina, INT/Intelligence, WIS/Wisdom");
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
	}
}