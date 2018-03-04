namespace RealmRpgBot.Models
{
	using System.Collections.Generic;

	public class CombatLog
	{
		public string Id { get; set; }
		public System.DateTime Timestamp { get; set; }
		public Combat.CombatOutcome Outcome { get; set; }
		public int Rounds { get; set; }
		public string Winner { get; set; }
		public string Loser { get; set; }
		public List<string> Lines { get; set; }
	}
}
