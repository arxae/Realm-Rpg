namespace RealmRpgBot.Models.Encounters
{
	using Character;

	public class EncounterTemplate
	{
		public string Id { get; set; }
		public string TemplateName { get; set; }

		public bool AdjustToPlayerLevel { get; set; }
		public int LevelRangeMin { get; set; }
		public int LevelRangeMax { get; set; }

		public bool AutoHp { get; set; }
		public int BaseHp { get; set; }
		public int HpVariance { get; set; }

		public AttributeBlock Attributes { get; set; }

		public override string ToString() => $"{TemplateName} ({Id})";
	}
}