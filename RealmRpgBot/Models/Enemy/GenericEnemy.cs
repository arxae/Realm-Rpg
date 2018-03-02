namespace RealmRpgBot.Models.Enemy
{
	using RealmRpgBot.Models.Character;

	public class GenericEnemy : Combat.IBattleParticipant
	{
		public string Name { get; set; }
		public int Level { get; set; }
		public int HpMax { get; set; }
		public int HpCurrent { get; set; }
		public AttributeBlock Attributes { get; set; }

		public string TemplateName { get; set; }

		public void ApplyTemplate(EnemyTemplate tmp)
		{
			Attributes = tmp.Attributes;

			Level = DiceNotation.SingletonRandom.Instance.Next(tmp.LevelRangeMin, tmp.LevelRangeMax);

			if (DiceNotation.SingletonRandom.Instance.Next(0, 100) > 50)
			{
				HpMax = tmp.BaseHp + DiceNotation.SingletonRandom.Instance.Next(0, 3);
			}
			else
			{
				HpMax = tmp.BaseHp - DiceNotation.SingletonRandom.Instance.Next(0, 3);
			}

			HpCurrent = HpMax;
		}
	}
}