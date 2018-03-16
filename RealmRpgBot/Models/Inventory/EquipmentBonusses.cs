namespace RealmRpgBot.Models.Inventory
{
	public struct EquipmentBonusses
	{
		public int AttackBonus { get; }
		public int DefenceBonus { get; }

		public EquipmentBonusses(int attackBonus, int defenceBonus)
		{
			AttackBonus = attackBonus;
			DefenceBonus = defenceBonus;
		}

		public static EquipmentBonusses operator +(EquipmentBonusses a, EquipmentBonusses b)
			=> new EquipmentBonusses(a.AttackBonus + b.AttackBonus, a.DefenceBonus + b.DefenceBonus);

		public static EquipmentBonusses operator -(EquipmentBonusses a, EquipmentBonusses b)
			=> new EquipmentBonusses(a.AttackBonus - b.AttackBonus, a.DefenceBonus - b.DefenceBonus);
	}
}
