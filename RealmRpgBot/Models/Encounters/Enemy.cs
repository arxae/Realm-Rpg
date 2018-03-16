namespace RealmRpgBot.Models.Encounters
{
	using Character;
	using Inventory;

	public class Enemy : Combat.IBattleParticipant
	{
		public string Name { get; set; }
		public int Level { get; set; }
		public int HpMax { get; set; }
		public int HpCurrent { get; set; }
		public AttributeBlock Attributes { get; set; }
		public EquipSet Equipment { get; set; }

		public string TemplateId { get; set; }

		public Enemy(EncounterTemplate tmp, int playerLevel)
		{
			Name = tmp.TemplateName;

			if (tmp.AdjustToPlayerLevel)
			{
				Level = Rng.Instance.Next(0, 100) > 50
					? playerLevel + Rng.Instance.Next(tmp.LevelRangeMin, tmp.LevelRangeMax)
					: playerLevel - Rng.Instance.Next(tmp.LevelRangeMin, tmp.LevelRangeMax);
			}
			else
			{
				Level = Rng.Instance.Next(tmp.LevelRangeMin, tmp.LevelRangeMax);
			}

			if (Level < 1) Level = 1;

			if (tmp.AutoHp)
			{
				HpMax = Rng.Instance.Next(0, 100) > 50
					? Rpg.GetBaseHpForLevel(Level) + Rng.Instance.Next(0, tmp.HpVariance)
					: Rpg.GetBaseHpForLevel(Level) - Rng.Instance.Next(0, tmp.HpVariance);
			}
			else
			{
				HpMax = Rng.Instance.Next(0, 100) > 50
					? tmp.BaseHp + Rng.Instance.Next(0, tmp.HpVariance)
					: tmp.BaseHp - Rng.Instance.Next(0, tmp.HpVariance);
			}

			Attributes = tmp.Attributes ?? new AttributeBlock(1);
			Equipment = tmp.Equipment ?? new EquipSet();

			HpCurrent = HpMax;

			TemplateId = tmp.Id;
		}

		public EquipmentBonusses GetEquipmentBonusses()
		{
			return new EquipmentBonusses(Attributes.Strength, Attributes.Stamina) + Equipment.GetCurrentBonusses();
		}
	}
}