using RealmRpgBot.Combat;

namespace RealmRpgBot.Bot.Commands
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;
	using Raven.Client.Documents;

	using Models.Character;
	using Models.Enemy;
	using Models.Map;

	[Group("dev"), Description("Game administration commands"), RequireRoles(RoleCheckMode.Any, "Realm Admin")]
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
				var player = await session.LoadAsync<Player>(mention.Id.ToString());
				if (player == null)
				{
					await c.RespondAsync(Constants.MSG_USER_NOT_REGISTERED);
					await c.RejectMessage();

					return;
				}

				await player.AddXpAsync(xpAmount, c);
				await session.SaveChangesAsync();

				await Realm.LogHistory(c.User.GetFullUsername(), mention.GetFullUsername(), c.Command.QualifiedName, xpAmount.ToString());

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
					await c.RespondAsync(Constants.MSG_USER_NOT_REGISTERED);
					await c.RejectMessage();

					return;
				}

				var xpNeeded = player.XpNext - player.XpCurrent;
				await player.AddXpAsync(xpNeeded, c);
				await session.SaveChangesAsync();

				await Realm.LogHistory(c.User.GetFullUsername(), mention.GetFullUsername(), c.Command.QualifiedName);
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
					await c.RespondAsync(Constants.MSG_USER_NOT_REGISTERED);
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

				await Realm.LogHistory(c.GetFullUserName(), mention.GetFullUsername(), c.Command.QualifiedName, $"{mention.GetFullUsername()} ({mention.Id})", locationName);
			}

			await c.ConfirmMessage();
		}

		[Command("clearcache"), Aliases("cc"), Description("Clear the settings cache")]
		public async Task ClearSettingsCache(CommandContext c)
		{
			Realm.ClearSettingsCache();
			await c.ConfirmMessage();
		}

		[Command("combattest")]
		public async Task Test(CommandContext c)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());
				var monster = await session
					.Include<EnemyTemplate>(t => t.Id)
					.LoadAsync<GenericEnemy>("enemies/testmonster");
				var enemyTemplate = await session.LoadAsync<EnemyTemplate>(monster.TemplateName);
				monster.ApplyTemplate(enemyTemplate);

				await c.RespondAsync($"{c.User.Mention} has encountered a lvl{monster.Level} {monster.Name}");

				var combat = new Battle(player, monster, c);
				await combat.DoCombatAsync();

				switch (combat.AttackerResult)
				{
					case Battle.CombatResult.Win:
						await c.RespondAsync($"{c.User.Mention} was victorious {combat.Round} round(s) of combat with {player.HpCurrent}hp left.");
						await player.AddXpAsync(2, c); // TODO: Temporary random xp
						break;
					case Battle.CombatResult.Tie:
						await c.RespondAsync($"{c.User.Mention} and {monster.Name} knocked each other out");
						await player.AddXpAsync(2, c); // TODO: Temporary random xp
						await player.SetFaintedAsync();
						break;
					default:
						await c.RespondAsync($"{c.User.Mention} has fainted after {combat.Round} round(s) of combat. {monster.Name} had {monster.HpCurrent}hp left");
						await player.SetFaintedAsync();
						break;
				}

				if (session.Advanced.HasChanges)
				{
					session.Advanced.IgnoreChangesFor(monster);
					await session.SaveChangesAsync();
				}
			}
		}
	}
}
