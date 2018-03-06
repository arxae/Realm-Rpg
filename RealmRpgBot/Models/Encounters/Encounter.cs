namespace RealmRpgBot.Models.Encounters
{
using System.Collections.Generic;

    public class Encounter
    {
        public string Id { get; set; }
		public string Description { get; set; }
		public List<string> Templates { get; set; }
		public EncounterTypes EncounterType { get; set; }

	    public enum EncounterTypes
	    {
			Enemy
	    }
    }
}
