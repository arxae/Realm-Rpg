namespace RealmRpgBot.Models.Enemy
{
	using Character;

	public class Enemy : Combat.IBattleParticipant
	{
		public string Name { get; set; }
		public int Level { get; set; }
		public int HpMax { get; set; }
		public int HpCurrent { get; set; }
		public AttributeBlock Attributes { get; set; }

		public string TemplateId { get; set; }

		public Enemy(string name, EnemyTemplate tmp)
		{
			Name = name;
			Attributes = tmp.Attributes;

			Level = DiceNotation.SingletonRandom.Instance.Next(tmp.LevelRangeMin, tmp.LevelRangeMax);

			if (DiceNotation.SingletonRandom.Instance.Next(0, 100) > 50)
			{
				HpMax = tmp.BaseHp + DiceNotation.SingletonRandom.Instance.Next(0, tmp.HpVariance);
			}
			else
			{
				HpMax = tmp.BaseHp - DiceNotation.SingletonRandom.Instance.Next(0, tmp.HpVariance);
			}

			HpCurrent = HpMax;

			TemplateId = tmp.Id;
		}
	}
}