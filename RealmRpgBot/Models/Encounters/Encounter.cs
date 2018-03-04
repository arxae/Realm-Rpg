namespace RealmRpgBot.Models.Encounters
{
    public class Encounter
    {
        public string Id { get; set; }
		public string Name { get; set; }
		public string TemplateId { get; set; }
		public EncounterTypes EncounterType { get; set; }

	    public enum EncounterTypes
	    {
			Enemy
	    }
    }
}
