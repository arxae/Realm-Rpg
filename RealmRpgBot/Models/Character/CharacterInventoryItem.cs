namespace RealmRpgBot.Models.Character
{
	public class CharacterInventoryItem
	{
		public string ItemId { get; set; }
		public int Amount { get; set; }

		public CharacterInventoryItem() { }
		public CharacterInventoryItem(string itemId, int amount)
		{
			ItemId = itemId;
			Amount = amount;
		}
	}
}
