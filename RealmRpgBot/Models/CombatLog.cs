namespace RealmRpgBot.Models
{
	using System.Collections.Generic;

	public class CombatLog
	{
		public string Id { get; set; }
		public System.DateTime Timestamp { get; set; }
		public Combat.Battle.CombatResult Result { get; set; }
		public int Rounds { get; set; }
		public string Victor { get; set; }
		public string Loser { get; set; }
		public List<string> Lines { get; set; }
	}
}
