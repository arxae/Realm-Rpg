namespace RealmRpgBot.Skills
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	using Models;

	public interface ISkill
	{
		Task ExecuteSkill(CommandContext c, Skill skill, Player source, object target);
	}
}