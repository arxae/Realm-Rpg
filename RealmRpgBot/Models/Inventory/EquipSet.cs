namespace RealmRpgBot.Models.Inventory
{
	using System;

	public class EquipSet
	{
		public string Head { get; set; }
		public string Chest { get; set; }
		public string Hands { get; set; }
		public string Shoulders { get; set; }
		public string Pants { get; set; }
		public string Feet { get; set; }

		public string MainHand { get; set; }
		public string OffHand { get; set; }

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
				case Item.EquipmentSlots.Hands:
					Hands = itemId;
					break;
				case Item.EquipmentSlots.Shoulders:
					Shoulders = itemId;
					break;
				case Item.EquipmentSlots.Pants:
					Pants = itemId;
					break;
				case Item.EquipmentSlots.Feet:
					Feet = itemId;
					break;
				case Item.EquipmentSlots.MainHand:
					MainHand = itemId;
					break;
				case Item.EquipmentSlots.OffHand:
					OffHand = itemId;
					break;
			}
		}

		public EquipmentBonusses GetCurrentBonusses()
		{
			using (var session = Db.DocStore.OpenSession())
			{
				int att = 0;
				int def = 0;

				var head = session.Load<Item>(Head);
				var chest = session.Load<Item>(Chest);
				var hands = session.Load<Item>(Hands);
				var shoulders = session.Load<Item>(Shoulders);
				var pants = session.Load<Item>(Pants);
				var feet = session.Load<Item>(Feet);
				var mhand = session.Load<Item>(MainHand);
				var ohand = session.Load<Item>(OffHand);

				if (head != null) { att += head.AttackBonus; def += head.DefenceBonus; }
				if (chest != null) { att += head.AttackBonus; def += head.DefenceBonus; }
				if (hands != null) { att += hands.AttackBonus; def += hands.DefenceBonus; }
				if (shoulders != null) { att += shoulders.AttackBonus; def += shoulders.DefenceBonus; }
				if (pants != null) { att += pants.AttackBonus; def += pants.DefenceBonus; }
				if (feet != null) { att += feet.AttackBonus; def += feet.DefenceBonus; }
				if (mhand != null) { att += mhand.AttackBonus; def += mhand.DefenceBonus; }
				if (ohand != null) { att += ohand.AttackBonus; def += ohand.DefenceBonus; }

				return new EquipmentBonusses(att, def);
			}
		}
	}
}
