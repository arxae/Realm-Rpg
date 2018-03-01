namespace RealmRpgBot.Skills
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	using Models.Character;

	public interface ISkill
	{
		bool DoCooldown { get; set; }
		Task ExecuteSkill(CommandContext c, Skill skill, TrainedSkill trainedSkill, Player source, object target);
	}
}