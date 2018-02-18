namespace RealmRpgBot.Models
{
	using System.Collections.Generic;

	public class BuildingAction
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public string ReactionIcon { get; set; }
		public List<string> ActionCommands { get; set; }
	}
}
