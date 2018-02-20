namespace RealmRpgBot.Models
{
	using DSharpPlus.Entities;

	public class Player
	{
		// Identification
		public string Id { get; set; }
		public string UserName { get; set; }

		// General Info
		public string Race { get; set; }
		public int Level { get; set; }
		public int HpMax { get; set; }
		public int HpCurrent { get; set; }

		// Progression
		public int XpCurrent { get; set; }
		public int XpNext { get; set; }

		// Location
		public string CurrentLocation { get; set; }

		public Player() { }
		public Player(DiscordUser user, string race)
		{
			Id = user.Id.ToString();
			UserName = user.GetFullUsername();
			Race = race;
			Level = 1;
			XpCurrent = 0;
		}
	}
}
// =10*B5^2+10*B5
// (10 * (LEVEL+1)^2) + (10*(LEVEL+1))