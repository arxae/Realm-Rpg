namespace RealmRpgBot.Models.Character
{
	using System.Collections.Generic;

    public class Race
    {
        public string Id { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }
		public List<string> StartingSkills { get; set; }
		public AttributeBlock BonusStats { get; set; }
    }
}
