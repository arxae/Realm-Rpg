namespace RealmRpgBot.Models.Map
{
	using System.Collections.Generic;

	public class Resource
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public string HarvestedItemId { get; set; }
		public int HarvestQuantityMin { get; set; }
		public int HarvestQuantityMax { get; set; }
		public List<string> AdditionalItems { get; set; }
		public int AdditionalItemsQuantityMin { get; set; }
		public int AdditionalItemsQuantityMax { get; set; }
		public int AdditionalItemsDificulty { get; set; }
	}
}
