namespace RealmRpgBot.Bot.Commands
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;

	using Models.Character;

	[Group("set"), RequireRoles(RoleCheckMode.All, "Realm Player", "Realm Admin")]
	public class SetCommands : RpgCommandBase
	{
		[Command("repeat"), Description("Repeat a action (if it allows it), until the setting is set back to off. Stopping an action automatically turns of repeat")]
		public async Task SetActionRepeat(CommandContext c,
			[Description("If set to on, the current action will be repeated when it ends (if allowed")] string repeatString)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());
				if (repeatString.ToLower() == "on" || repeatString.ToLower() == "true")
				{
					player.CurrentActionRepeat = true;
					await c.ConfirmMessage();
				}
				else if (repeatString.ToLower() == "off" || repeatString.ToLower() == "true")
				{
					player.CurrentActionRepeat = false;
					await c.ConfirmMessage();
				}
				else
				{
					await c.RejectMessage();
				}

				if (session.Advanced.HasChanges)
				{
					await session.SaveChangesAsync();
				}
			}
		}
	}
}
