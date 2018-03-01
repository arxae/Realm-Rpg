namespace RealmRpgBot.Models.Map

{
	using System;

	public class LocationInventoryItem
	{
		public string DocId { get; set; }
		public string DisplayName { get; set; }
		public int Amount { get; set; }
		public DateTime DecaysOn { get; set; }
	}
}
