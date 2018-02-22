namespace RealmRpgBot.Skills
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	using RealmRpgBot.Models;

	public class TestSkill : ISkill
	{
		public async Task ExecuteSkill(CommandContext c, Skill skill, Player source, object target)
		{
			// <@83703349525872640> -> User
			// <#402615348592902144> -> Channel
			// <@&400452230781861898> -> Role

			string tar = (string)target;
			if(tar.StartsWith("<@"))
			{
				// User
				string targetId = tar.Replace("<@", "").Replace(">", "");
				
				if(targetId == source.Id)
				{
					await c.RespondAsync($"{c.User.Mention} starts playing with himself. Ain't nobody wanna see that");
				}
				else
				{
					await c.RespondAsync($"{c.User.Mention} plays with {target} witouth asking. Nasty");
				}
			}
			else if(tar.StartsWith("<#"))
			{
				await c.RespondAsync($"{c.User.Mention} starts yelling loudly at everyone in the area, kinda obnoxious to be honest");
			}
			else
			{
				await c.RespondAsync($"{c.User.Mention} is dancing around, all by his lonesome self. Weirdo");
			}

			await c.ConfirmMessage();
		}
	}
}
