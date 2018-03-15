namespace RealmRpgBot.Models.Inventory
{
	using System;

	public class EquipSet
	{
		public string Head { get; set; }
		public string Chest { get; set; }

		public void EquipItem(string itemId, string slotString)
		{
			if (Enum.TryParse(slotString, out Item.EquipmentSlots slot) == false)
			{
				Serilog.Log.ForContext<EquipSet>().Error("Error while parsing equipment slot ({slot}) for item {id}", slotString, itemId);
			}

			switch (slot)
			{
				case Item.EquipmentSlots.Head:
					Head = itemId;
					break;
				case Item.EquipmentSlots.Chest:
					Chest = itemId;
					break;
			}
		}
	}
}
