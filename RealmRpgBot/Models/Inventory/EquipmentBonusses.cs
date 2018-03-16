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
	}
}
