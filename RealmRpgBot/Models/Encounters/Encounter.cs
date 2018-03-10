namespace RealmRpgBot.Models.Encounters
{
	using System.Collections.Generic;

	public class Encounter
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public List<string> Templates { get; set; }
		public EncounterTypes EncounterType { get; set; }
		public int XpReward { get; set; }

		public int GetActualXpReward(int playerLevel, int enemyLevel) => XpReward / 100 * Rpg.GetGainedXpModifier(playerLevel, enemyLevel);

		public enum EncounterTypes
		{
			Enemy
		}
	}
}
