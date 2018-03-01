namespace RealmRpgBot.Models.Map
{
	public class Resource
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public string HarvestedItemId { get; set; }
		public int HarvestQuantityMin { get; set; }
		public int HarvestQuantityMax { get; set; }
	}
}
