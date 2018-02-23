namespace RealmRpgBot.Models
{
	using DSharpPlus.Entities;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public class Player
	{
		// Identification
		public string Id { get; set; }
		public ulong GuildId { get; set; }
		public string UserName { get; set; }
		public string UserDiscriminator { get; set; }

		// General Info
		public int Level { get; set; }
		public string Class { get; set; }
		public string Race { get; set; }
		public AttributeBlock Attributes { get; set; }

		// Vitality
		public int HpMax { get; set; }
		public int HpCurrent { get; set; }

		// Progression
		public int XpCurrent { get; set; }
		public int XpNext { get; set; }
		public int SkillPoints { get; set; }
		public int AttributePoints { get; set; }
		public List<TrainedSkill> Skills { get; set; }

		// Location
		public string CurrentLocation { get; set; }

		public Player() { }
		public Player(DiscordUser user, DiscordGuild guild, string race)
		{
			Id = user.Id.ToString();
			GuildId = guild.Id;
			UserName = user.Username;
			UserDiscriminator = user.Discriminator;

			Level = 1;
			Race = race;
			Class = Realm.GetSetting<string>("startingclass");
			Attributes = new AttributeBlock(1);

			HpMax = Realm.GetBaseHpForLevel(1);
			HpCurrent = HpMax;

			XpCurrent = 0;
			XpNext = Realm.GetNextXp(1);
			Skills = new List<TrainedSkill>();

			CurrentLocation = Realm.GetSetting<string>("startinglocation");
		}

		public async Task AddXpAsync(int amount, DSharpPlus.CommandsNext.CommandContext c = null)
		{
			bool hasLeveled = false;
			XpCurrent += amount;

			if (XpCurrent >= XpNext)
			{
				hasLeveled = true;

				while (XpCurrent >= XpNext)
				{
					XpCurrent = XpCurrent - XpNext;
					Level++;
					XpNext = Realm.GetNextXp(Level);

					SkillPoints += Realm.GetSetting<int>("skillpointsperlevelup");
					AttributePoints += Realm.GetSetting<int>("attributepointsperlevelup");
				}
			}

			if (hasLeveled == false || c == null) return;

			var g = await Bot.RpgBot.Client.GetGuildAsync(GuildId);
			var m = await g.GetMemberAsync(ulong.Parse(Id));

			await c.Channel.SendMessageAsync($"{m.Mention} is now level {Level}!");
		}
	}
}