namespace RealmRpgBot.Skills
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	using Models;

	public class TestSkill : ISkill
	{
		public bool DoCooldown { get; set; }

		public async Task ExecuteSkill(CommandContext c, Skill skill, TrainedSkill trainedskill, Player source, object target)
		{
			var targetType = Realm.GetTargetTypeFromId((string)target);

			if (targetType == Enums.TargetType.User)
			{
				string targetId = Realm.GetIdFromMentionString((string)target);

				if (targetId == source.Id)
				{
					await c.RespondAsync($"{c.User.Mention} starts playing with himself. Ain't nobody wanna see that");
				}
				else
				{
					await c.RespondAsync($"{c.User.Mention} plays with {target} witouth asking. Nasty");
				}
			}
			else if (targetType == Enums.TargetType.Channel)
			{
				await c.RespondAsync($"{c.User.Mention} starts yelling loudly at everyone in the area, kinda obnoxious to be honest");
			}
			else
			{
				await c.RespondAsync($"{c.User.Mention} is dancing around, all by his lonesome self. Weirdo");
			}

			DoCooldown = true;

			await c.ConfirmMessage();
		}
	}
}
