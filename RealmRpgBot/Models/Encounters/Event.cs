namespace RealmRpgBot.Models.Encounters
{
	using System.Collections.Generic;

    public class Event
    {
        public string Id { get; set; }
		public string EventName { get; set; }
		public List<EventStage> Stages { get; set; }
    }
}
