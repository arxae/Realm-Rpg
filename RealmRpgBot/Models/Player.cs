namespace RealmRpgBot.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.Entities;

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
		public List<CharacterInventoryItem> Inventory { get; set; }

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

		// Actions
		public string CurrentAction { get; set; }
		public string CurrentActionDisplay { get; set; }
		public DateTime BusyUntil { get; set; }

		public bool IsIdle { get => CurrentAction.Equals("idle", StringComparison.OrdinalIgnoreCase); }

		public Player() { }
		public Player(DiscordUser user, DiscordGuild guild, string race)
		{
			Id = user.Id.ToString();
			GuildId = guild.Id;
			UserName = user.Username;
			UserDiscriminator = user.Discriminator;

			Inventory = new List<CharacterInventoryItem>();

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

			CurrentAction = "Idle";
			CurrentActionDisplay = "Idling";
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

		public void AddItemToInventory(string itemId, int amount)
		{
			var inv = Inventory.FirstOrDefault(pi => pi.ItemId == itemId);
			if (inv == null)
			{
				Inventory.Add(new CharacterInventoryItem(itemId, amount));
			}
			else
			{
				inv.Amount += amount;
			}
		}

		public void SetIdleAction()
		{
			CurrentAction = "Idle";
			CurrentActionDisplay = "Idling";
		}
	}
}