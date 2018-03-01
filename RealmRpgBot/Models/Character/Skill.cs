namespace RealmRpgBot.Models.Character
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
		public bool IsActivatable { get; set; }
		public List<int> TrainingCosts { get; set; }
		public List<int> CooldownRanks { get; set; }
		public Dictionary<string, string> Parameters { get; set; }
	}
}