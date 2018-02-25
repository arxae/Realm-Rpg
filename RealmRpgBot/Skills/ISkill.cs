namespace RealmRpgBot.Skills
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	using Models;

	public interface ISkill
	{
		Task ExecuteSkill(CommandContext c, Skill skill, TrainedSkill trainedSkill, Player source, object target);
	}
}