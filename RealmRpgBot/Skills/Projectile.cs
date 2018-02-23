namespace RealmRpgBot.Skills
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	using RealmRpgBot.Models;

	public class Projectile : ISkill
	{
		public async Task ExecuteSkill(CommandContext c, Skill skill, Player source, object target)
		{
			if (target == null)
			{
				await c.RespondAsync($"{skill.DisplayName} requires a target");
				await c.RejectMessage();
				return;
			}

			if(Realm.GetTargetTypeFromId((string)target)!= Enums.TargetType.User)
			{
				await c.RespondAsync("That is not a valid target");
				await c.RejectMessage();
				return;
			}

			await new ActionsProcessor(c, c.Message).ProcessActionList(skill.ActionCommands);
		}
	}
}
