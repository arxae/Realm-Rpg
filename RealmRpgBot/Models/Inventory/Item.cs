namespace RealmRpgBot.Models.Inventory
{
	/// <summary>
	/// Item definition
	/// </summary>
    public class Item
    {
        public string Id { get; set; }
		public string DisplayName { get; set; }
		public ItemTypes Type { get; set; }

	    public enum ItemTypes
	    {
			Junk,
			Recipe,
			Resource,
			Consumable
	    }
    }
}
