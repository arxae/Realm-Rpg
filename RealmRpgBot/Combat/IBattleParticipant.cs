namespace RealmRpgBot.Combat
{
	using Models.Character;

	public interface IBattleParticipant
	{
		string Name { get; set; }
		int Level { get; set; }
		int HpMax { get; set; }
		int HpCurrent { get; set; }

		AttributeBlock Attributes { get; set; }
	}
}
