namespace RealmRpgBot.Models
{
	using System.Collections.Generic;

	public class Skill
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public string ReactionIcon { get; set; }
		public string ImageUrl { get; set; }
		public string SkillImpl { get; set; }
		public List<int> TrainingCosts { get; set; }
		public List<string> ActionCommands { get; set; }
	}
}
