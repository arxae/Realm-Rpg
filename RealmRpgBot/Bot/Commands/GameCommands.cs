namespace RealmRpgBot.Bot.Commands
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using Raven.Client.Documents;

	using Models.Character;

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

				var raceInfo = await session.Query<Race>().FirstOrDefaultAsync(r => r.Id == $"races/{race}");
				if (raceInfo == null)
				{
					await c.RejectMessage($"{c.Member.Mention} Race \"{race}\" is not valid. You can use *.info races* to get a list of races");
					return;
				}

				var charClass = await session.Query<CharacterClass>().FirstOrDefaultAsync(cls => cls.Id == Realm.GetSetting<string>("startingclass"));
				if (charClass == null)
				{
					await c.RespondAsync($"Something went wrong while getting the startingclass (Error_StartingClassNull {Realm.GetSetting<string>("startingclass")})");
					return;
				}

				var player = Realm.GetPlayerRegistration(c.Member, raceInfo, charClass);

				await session.StoreAsync(player);
				await session.SaveChangesAsync();
			}

			var role = c.Guild.Roles.FirstOrDefault(r => r.Name == "Realm Player");
			if (role == null)
			{
				await c.RejectMessage(Realm.GetMessage("missing_player_role"));
				return;
			}

			await c.Member.GrantRoleAsync(role, "Registered for Realm");

			await c.RespondAsync($"{c.User.Mention} has joined the Realm");
			await c.ConfirmMessage();
		}

		[Command("remove"), Description("Remove yourself"), RequireRoles(RoleCheckMode.Any, Constants.ROLE_PLAYER)]
		public async Task RemovePlayer(CommandContext c)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync(Realm.GetMessage("not_registered"));
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
