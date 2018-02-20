namespace RealmRpgBot
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class Realm
	{
		static List<Type> _buildingImplementations;

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

		public static void SetProperty(object target, string propertyName, object value)
		{
			var prop = target.GetType().GetProperty(propertyName);
			prop.SetValue(target, value);
		}

		public static int GetNextXp(int level)
		{
			int levelFactor = 2; // TODO: Put into config/db
			return levelFactor * (int)Math.Pow((level + 1), 2) + levelFactor * (level + 1);
		}
	}
}
