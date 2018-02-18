namespace RealmRpgBot.Bot.Commands
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using Raven.Client.Documents;

	using Models;

	/// <summary>
	/// Various actions player can perform
	/// </summary>
	public class GeneralActionCommands : RpgCommandBase
	{
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
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());

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
		/// <param name="locName"></param>
		/// <returns></returns>
		[Command("travel"), Description("Travel to a specific location that is linked to the current location")]
		public async Task Travel(CommandContext c,
			[Description("Name of the destination"), RemainingText] string locName)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include("Player.CurrentLocation")
					.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync($"{c.User.Mention}, {Constants.MSG_NOT_REGISTERED}");
					await c.RejectMessage();

					return;
				}

				var location = await session.LoadAsync<Location>(player.CurrentLocation);

				var dest = await session.Query<Location>().FirstOrDefaultAsync(tl => tl.DisplayName == locName);

				if (dest != null)
				{
					player.CurrentLocation = dest.Id;
					await session.SaveChangesAsync();

					await c.RespondAsync($"{c.User.Mention} has arrived in {dest.DisplayName}");
					await c.ConfirmMessage();
				}
				else
				{
					await c.RespondAsync($"{locName} is not a valid destination");
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
					.Include("Player.CurrentLocation")
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
				if(bType == null)
				{
					await c.RespondAsync("An error occured. Contact one of the admins and mention: Error_GeneralActions_BuildingImplNotFound");
					await c.RejectMessage();

					return;
				}

				var bImpl = (Buildings.IBuilding)System.Activator.CreateInstance(bType);
				await bImpl.EnterBuilding(c, building);

				await c.ConfirmMessage();
			}
		}
	}
}
