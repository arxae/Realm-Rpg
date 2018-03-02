namespace RealmRpgBot.Models.Enemy
{
	using Character;

	public class EnemyTemplate
	{
		public string Id { get; set; }
		public string TemplateName { get; set; }

		public int LevelRangeMin { get; set; }
		public int LevelRangeMax { get; set; }

		public int BaseHp { get; set; }
		public int HpVariance { get; set; }

		public AttributeBlock Attributes { get; set; }
	}
}