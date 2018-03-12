namespace RealmRpgBot.Models.Character
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using Combat;

	public class Player : IBattleParticipant
	{
		// Identification
		public string Id { get; set; }
		public ulong GuildId { get; set; }
		public string Name { get; set; }

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

		// Locations
		public string PreviousLocation { get; set; }
		public string CurrentLocation { get; set; }
		public List<string> FoundHiddenLocations { get; set; }
		public Dictionary<string, int> LocationExploreCounts { get; set; }

		// Actions
		public string CurrentAction { get; set; }
		public string CurrentActionDisplay { get; set; }
		public bool CurrentActionRepeat { get; set; }
		public DateTime BusyUntil { get; set; }

		public bool IsIdle => CurrentAction.Equals("idle", StringComparison.OrdinalIgnoreCase);

		public Player()
		{
			Inventory = new List<CharacterInventoryItem>();
			Skills = new List<TrainedSkill>();
			FoundHiddenLocations = new List<string>();
			LocationExploreCounts = new Dictionary<string, int>();
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
					XpNext = Rpg.GetNextXp(Level);

					SkillPoints += Realm.GetSetting<int>("skill_points_per_levelup");
					AttributePoints += Realm.GetSetting<int>("attribute_points_per_levelup");
				}
			}

			if (hasLeveled == false || c == null) return;

			HpMax = Rpg.GetBaseHpForLevel(Level);
			HpCurrent = HpMax;

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
			CurrentActionRepeat = false;
			CurrentAction = "Idle";
			CurrentActionDisplay = "Idling";
		}

		public async Task<int> SetFaintedAsync()
		{
			return await Task.Run(() =>
			{
				CurrentLocation = Realm.GetSetting<string>("startinglocation");
				HpCurrent = HpMax;

				var percentage = Realm.GetSetting<int>("faint_xp_penalty");
				var xpLoss = (XpCurrent / 100) * percentage;

				XpCurrent = XpCurrent - xpLoss;
				if (XpCurrent < 0) XpCurrent = 0;

				return xpLoss;
			});
		}

		public async Task SetActionAsync(string action, string actionDisplay, TimeSpan time)
		{
			await Task.Run(() =>
			{
				CurrentAction = action;
				CurrentActionDisplay = actionDisplay;
				BusyUntil = DateTime.Now + time;
			});
		}

		public async Task HealHpAsync(int hp)
		{
			await Task.Run(() =>
			{
				HpCurrent += hp;
				if (HpCurrent > HpMax) HpCurrent = HpMax;
			});
		}

		public void AddSkill(TrainedSkill skill)
		{
			if (Skills.Any(s => s.Id == skill.Id) == false)
			{
				Skills.Add(skill);
				return;
			}

			Serilog.Log.ForContext<Player>().Warning("Tried adding  {skill} to player {n} ({id}), but was already added", skill.Id, Name, Id);
		}
	}
}