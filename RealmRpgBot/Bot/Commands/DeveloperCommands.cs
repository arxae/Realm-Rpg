using Raven.Client.Documents.Attachments;
using Raven.Client.Documents.Operations.Attachments;

namespace RealmRpgBot.Bot.Commands
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;
	using Raven.Client.Documents;

	using Models.Character;
	using Models.Encounters;
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

		[Command("test")]
		public async Task Test(CommandContext c)
		{
			using (var s = Db.DocStore.OpenAsyncSession())
			{
				var doc = await s.LoadAsync<Models.Setting>("test/testdoc");


				//var attachment = await Db.DocStore.Operations.SendAsync(new GetAttachmentOperation(buildingActionId, "action.lua", AttachmentType.Document, null));
				//string script = await new System.IO.StreamReader(attachment.Stream).ReadToEndAsync();

				var att = await Db.DocStore.Operations.SendAsync(new GetAttachmentOperation("test/testdoc", "action.lua", AttachmentType.Document, null));
				string script = await new System.IO.StreamReader(att.Stream).ReadToEndAsync();

				var scriptR = new ScriptRunner(c, null);
				await scriptR.PerformScriptAsync(script);



			}
		}
	}
}
