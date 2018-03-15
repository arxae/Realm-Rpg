namespace RealmRpgBot.Models.Inventory
{
    public struct EquipmentBonusses
    {
        public int AttackBonus { get; }
        public int DefenceBonus{ get;}

	    public EquipmentBonusses(int ab, int db)
	    {
			AttackBonus = ab;
			DefenceBonus = db;
	    }
	}
}
