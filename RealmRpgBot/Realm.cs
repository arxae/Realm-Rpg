namespace RealmRpgBot
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class Realm
	{
		private static List<Type> _buildingImplementations;
		private static List<Type> _skillImplementations;
		private static List<Models.Setting> _settingsCache = new List<Models.Setting>();

		public static void ClearCacheForKey(string key)
		{
			var entry = _settingsCache.FirstOrDefault(s => s.Id.Equals(key, StringComparison.OrdinalIgnoreCase));
			if (entry == null) return;

			Serilog.Log.ForContext<Realm>().Debug("Removing cache value for {key}", key);

			_settingsCache.Remove(entry);
		}

		public static Type FindType(string typeName)
		{
			try
			{
				var t = Type.GetType(typeName, true, true);
				return t;
			}
			catch
			{
				return null;
			}
		}

		public static int GetBaseHpForLevel(int currentLevel)
		{
			var increaseFactor = GetSetting<int>("hpincreasefactor");
			var startinghp = GetSetting<int>("startinghp");

			return (increaseFactor * currentLevel) * startinghp;
		}

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

		public static int GetNextXp(int currentLevel)
		{
			int levelFactor = GetSetting<int>("levelfactor");
			return levelFactor * (int)Math.Pow((currentLevel + 1), 2) + levelFactor * (currentLevel + 1);
		}

		public static T GetSetting<T>(string key)
		{
			var cached = _settingsCache.FirstOrDefault(c => c.Id.Equals(key, StringComparison.OrdinalIgnoreCase));

			if (cached != null)
			{
				Serilog.Log.ForContext<Realm>().Debug("Using cached value for {key}", key);
				return (T)Convert.ChangeType(cached.Value, typeof(T));
			}

			using (var session = Db.DocStore.OpenSession())
			{
				Serilog.Log.ForContext<Realm>().Debug("Caching {key} value", key);
				var setting = session.Query<Models.Setting>().FirstOrDefault(s => s.Id.Equals(key, StringComparison.OrdinalIgnoreCase));
				_settingsCache.Add(setting);

				return (T)Convert.ChangeType(setting.Value, typeof(T));
			}
		}

		public static void SetProperty(object target, string propertyName, object value)
		{
			var prop = target.GetType().GetProperty(propertyName);
			prop.SetValue(target, value);
		}

		public static void SetupDbSubscriptions()
		{
			// Invalidates settings cache when a setting changes
			Db.DocStore.Subscriptions.GetSubscriptionWorker<Models.Setting>("Settings Changed")
				.Run(s =>
				{
					foreach (var setting in s.Items) ClearCacheForKey(setting.Id);
				});
		}

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

		public static Enums.TargetType GetTargetTypeFromId(string id)
		{
			if (id == null) return Enums.TargetType.None;

			if (id.StartsWith("<@") && id.EndsWith(">")) return Enums.TargetType.User;
			if (id.StartsWith("<#") && id.EndsWith(">")) return Enums.TargetType.Channel;
			if (id.StartsWith("<@&") && id.EndsWith(">")) return Enums.TargetType.Role;

			return Enums.TargetType.None;
		}

		public static string GetIdFromMentionString(string mention)
		{
			string tmp;
			switch (GetTargetTypeFromId(mention))
			{
				case Enums.TargetType.User:
					tmp = mention.Replace("<@", "");
					tmp = tmp.Substring(0, tmp.LastIndexOf(">"));
					return tmp;
				case Enums.TargetType.Channel:
					tmp = mention.Replace("<#", "");
					tmp = tmp.Substring(0, tmp.LastIndexOf(">"));
					return tmp;
				case Enums.TargetType.Role:
					tmp = mention.Replace("<@&", "");
					tmp = tmp.Substring(0, tmp.LastIndexOf(">"));
					return tmp;
				default:
					return mention;
			}
		}
	}
}