namespace RealmRpgBot.Models
{
	using DSharpPlus.Entities;
	using System.Threading.Tasks;

	public class Player
	{
		// Identification
		public string Id { get; set; }
		public ulong GuildId { get; set; }
		public string UserName { get; set; }
		public string UserDiscriminator { get; set; }

		// General Info
		public string Race { get; set; }
		public int Level { get; set; }

		// Vitality
		public int HpMax { get; set; }
		public int HpCurrent { get; set; }

		// Progression
		public int XpCurrent { get; set; }
		public int XpNext { get; set; }

		// Location
		public string CurrentLocation { get; set; }

		public Player() { }
		public Player(DiscordUser user, DiscordGuild guild, string race)
		{
			Id = user.Id.ToString();
			GuildId = guild.Id;
			UserName = user.Username;
			UserDiscriminator = user.Discriminator;

			Race = race;

			HpMax = Realm.GetSetting<int>("startinghp");
			HpCurrent = HpMax;

			Level = 1;
			XpCurrent = 0;
			XpNext = Realm.GetNextXp(1);

			CurrentLocation = Realm.GetSetting<string>("startinglocation");
		}

		public async Task AddXpAsync(int amount, DSharpPlus.CommandsNext.CommandContext c = null)
		{
			bool hasLeveled = false;
			XpCurrent += amount;

			if (XpCurrent > XpNext)
			{
				hasLeveled = true;

				while (XpCurrent > XpNext)
				{
					XpCurrent = XpCurrent - XpNext;
					Level++;
					XpNext = Realm.GetNextXp(Level);
				}
			}

			if (hasLeveled == false || c == null) return;

			await c.Channel.SendMessageAsync($"{c.User.Mention} is now level {Level}!");
		}
	}
}