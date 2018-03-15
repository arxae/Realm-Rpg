namespace RealmRpgBot
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using DSharpPlus.Entities;

	using Models;
	using Models.Character;

	/// <summary>
	/// This class is used to hold various methods in regards to system functionality (settings, cache, etc...)
	/// </summary>
	public class Realm
	{
		static List<Type> _buildingImplementations;
		static List<Type> _skillImplementations;
		static readonly List<Setting> _settingsCache = new List<Setting>();

		/// <summary>
		/// Clears the cache entry for a specific setting
		/// </summary>
		/// <param name="key">The name of the setting.</param>
		public static void ClearCacheForKey(string key)
		{
			if (key.StartsWith("settings/") == false) key = "settings/" + key;
			var entry = _settingsCache.FirstOrDefault(s => s.Id.Equals(key, StringComparison.OrdinalIgnoreCase));
			if (entry == null) return;

			Serilog.Log.ForContext<Realm>().Debug("Removing cache value for {key}", key);

			_settingsCache.Remove(entry);
		}

		/// <summary>
		/// Clear the entire settings cache
		/// </summary>
		public static void ClearSettingsCache()
		{
			Serilog.Log.ForContext<Realm>().Debug("Removing {n} cached settings", _settingsCache.Count);
			_settingsCache.Clear();
		}

		/// <summary>
		/// Gets a building implementation based on the name.
		/// </summary>
		/// <param name="implName">Name of the building implementation</param>
		/// <returns>The <see cref="Type"/> of the implementation</returns>
		public static Type GetBuildingImplementation(string implName)
		{
			if (_buildingImplementations == null)
			{
				_buildingImplementations = new List<Type>();

				var interfaceType = typeof(Buildings.IBuilding);

				_buildingImplementations.AddRange(AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(a => a.GetTypes())
					.Where(t => t.GetInterfaces().Contains(interfaceType))
					.ToList());
			}

			return _buildingImplementations.FirstOrDefault(a => a.Name.Equals(implName, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Gets a skill implementation based on the name.
		/// </summary>
		/// <param name="implName">The name of the skill implementation</param>
		/// <returns>The <see cref="Type"/> of the implmentation</returns>
		public static Type GetSkillImplementation(string implName)
		{
			if (_skillImplementations == null)
			{
				_skillImplementations = new List<Type>();
				var interfaceType = typeof(Skills.ISkill);
				_skillImplementations.AddRange(AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(a => a.GetTypes())
					.Where(t => t.GetInterfaces().Contains(interfaceType))
					.ToList());
			}

			return _skillImplementations.FirstOrDefault(a => a.Name.Equals(implName, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Gets all the server urls from the server config file (Servers.txt in the root folder)
		/// </summary>
		/// <returns>A array of server urls</returns>
		public static string[] GetDbServerUrls()
		{
			if (File.Exists("Servers.txt") == false)
			{
				Serilog.Log.Logger.Fatal("Could not find servers.txt. Make sure this file exists and has at least 1 server in it. Press any key to exit");
				Console.ReadKey();
				Environment.Exit(1);
			}

			return File.ReadAllLines("Servers.txt");
		}

		/// <summary>
		/// Gets a specific setting and cache it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">Setting name (exlude the settings/ prefix)</param>
		/// <returns>Value of the setting</returns>
		public static T GetSetting<T>(string key)
		{
			if (key.StartsWith("settings/") == false) key = "settings/" + key;
			var cached = _settingsCache.FirstOrDefault(c => c.Id.Equals(key, StringComparison.OrdinalIgnoreCase));

			if (cached != null)
			{
				Serilog.Log.ForContext<Realm>().Debug("Using cached value for {key}", key);
				return (T)Convert.ChangeType(cached.Value, typeof(T));
			}

			using (var session = Db.DocStore.OpenSession())
			{
				Serilog.Log.ForContext<Realm>().Debug("Caching {key} value", key);

				var setting = session.Query<Setting>().FirstOrDefault(s => s.Id.Equals(key, StringComparison.OrdinalIgnoreCase));

				if (setting == null)
				{
					Serilog.Log.ForContext<Realm>().Error("Tried to retrieve not existing setting {name}", key);
					return default(T);
				}

				if (setting.Value.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
				{
					setting.Value = ((Newtonsoft.Json.Linq.JArray)setting.Value).ToObject<T>();
				}
				else if (setting.Value.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
				{
					setting.Value = ((Newtonsoft.Json.Linq.JObject)setting.Value).ToObject<T>();
				}

				_settingsCache.Add(setting);

				return (T)Convert.ChangeType(setting.Value, typeof(T));
			}
		}

		/// <summary>
		/// Gets a specific message from the settings
		/// </summary>
		/// <param name="msgName">The name of the message</param>
		/// <returns></returns>
		public static string GetMessage(string msgName)
		{
			var setting = GetSetting<Dictionary<string, string>>("messages");
			if (setting.ContainsKey(msgName)) return setting[msgName];
			Serilog.Log.ForContext<Realm>().Error("Could not find message {msgname}", msgName);
			return string.Empty;
		}

		/// <summary>
		/// Gets a specific emoji name from the settings
		/// </summary>
		/// <param name="emojiName">The name of the emoji</param>
		/// <returns></returns>
		public static string GetEmoji(string emojiName)
		{
			var setting = GetSetting<Dictionary<string, string>>("emoji_list");
			if (setting.ContainsKey(emojiName)) return setting[emojiName];
			Serilog.Log.ForContext<Realm>().Error("Could not find emoji {ename}", emojiName);
			return string.Empty;
		}

		/// <summary>
		/// Looks for the first .pfx file it finds in the root directory, and use that to connect to the database server.
		/// </summary>
		/// <returns>Filename of the certificate</returns>
		public static string GetCertificate()
		{
			var l = Serilog.Log.ForContext<Realm>();

			l.Information("Looking for certificate");
			var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.pfx").Where(item => item.EndsWith(".pfx")).ToArray();

			if (files.Length == 0)
			{
				l.Information("No certificate found");
				return string.Empty;
			}

			Serilog.Log.ForContext<Realm>().Information($"Found certificate: {Path.GetFileName(files[0])}");
			return files[0];
		}

		/// <summary>
		/// Gets if the id is a discord user, channel or role
		/// </summary>
		/// <param name="mention">The mention (in string form)</param>
		/// <returns>Enum value containing the type of the mention</returns>
		public static Enums.DiscordTargetType GetTargetTypeFromMention(string mention)
		{
			if (mention == null) return Enums.DiscordTargetType.None;

			if (mention.StartsWith("<@") && mention.EndsWith(">")) return Enums.DiscordTargetType.User;
			if (mention.StartsWith("<#") && mention.EndsWith(">")) return Enums.DiscordTargetType.Channel;
			if (mention.StartsWith("<@&") && mention.EndsWith(">")) return Enums.DiscordTargetType.Role;

			return Enums.DiscordTargetType.None;
		}

		/// <summary>
		/// Gets the id from a discord user, channel or role mention
		/// </summary>
		/// <param name="mention">The mention (in string form)</param>
		/// <returns>The id witouth the mention characters</returns>
		public static string GetIdFromMentionString(string mention)
		{
			string tmp;
			switch (GetTargetTypeFromMention(mention))
			{
				case Enums.DiscordTargetType.User:
					tmp = mention.Replace("<@", "");
					tmp = tmp.Substring(0, tmp.LastIndexOf(">", StringComparison.OrdinalIgnoreCase));
					return tmp;
				case Enums.DiscordTargetType.Channel:
					tmp = mention.Replace("<#", "");
					tmp = tmp.Substring(0, tmp.LastIndexOf(">", StringComparison.OrdinalIgnoreCase));
					return tmp;
				case Enums.DiscordTargetType.Role:
					tmp = mention.Replace("<@&", "");
					tmp = tmp.Substring(0, tmp.LastIndexOf(">", StringComparison.OrdinalIgnoreCase));
					return tmp;
				default:
					return mention;
			}
		}

		/// <summary>
		/// Prepares a player object to be stored into the database
		/// </summary>
		/// <param name="member">The discord member of the registering user</param>
		/// <param name="raceInfo">The raceinformation that the user registered with</param>
		/// <param name="classInfo">Class of the player</param>
		/// <returns>Complete player object (do note, this is not directly saved)</returns>
		public static Player GetPlayerRegistration(DiscordMember member, Race raceInfo, CharacterClass classInfo)
		{
			var p = new Player
			{
				Id = member.Id.ToString(),
				GuildId = member.Guild.Id,
				Name = member.Username,
				Level = 1,
				Race = raceInfo.Id,
				Class = classInfo.Id,
				Attributes = new AttributeBlock(1),
				HpMax = Rpg.GetBaseHpForLevel(1),
				ManaMax = Rpg.GetMaxMana(1, 1),
				XpCurrent = 0,
				XpNext = Rpg.GetNextXp(1),
				CurrentLocation = GetSetting<string>("startinglocation")
			};

			p.HpCurrent = p.HpMax;
			p.ManaCurrent = p.ManaMax;
			p.PreviousLocation = p.CurrentLocation;

			foreach (var skillId in raceInfo.StartingSkills)
			{
				var split = skillId.Split(':');
				if (split.Length == 1)
				{
					p.AddSkill(new TrainedSkill(skillId, 1));
					continue;
				}

				if (int.TryParse(split[1], out int rank) == false)
				{
					Serilog.Log.ForContext<Player>().Error("Error while parsing skill id {skill}", skillId);
					continue;
				}

				p.AddSkill(new TrainedSkill(split[0], rank));
			}

			foreach (var skill in GetSetting<List<string>>("global_starting_skills"))
			{
				p.AddSkill(new TrainedSkill(skill, 1));
			}

			p.Attributes += raceInfo.BonusStats + classInfo.BonusStats;

			p.SetIdleAction();

			return p;
		}
	}
}