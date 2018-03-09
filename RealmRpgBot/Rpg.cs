namespace RealmRpgBot
{
	using System;
	
	/// <summary>
	/// Utility methods for the rpg part of the game
	/// </summary>
	public static class Rpg
	{
		/// <summary>
		/// Gets the (max) HP based on level
		/// </summary>
		/// <param name="currentLevel">The level to calculate the value from</param>
		/// <returns>The (max) HP for this level</returns>
		public static int GetBaseHpForLevel(int currentLevel)
		{
			var increaseFactor = Realm.GetSetting<int>("hpincreasefactor");
			var startinghp = Realm.GetSetting<int>("startinghp");

			return (increaseFactor * currentLevel) * startinghp;
		}

		/// <summary>
		/// Gets the XP needed for the next level
		/// </summary>
		/// <param name="currentLevel">The current level</param>
		/// <returns>XP needed for the next level</returns>
		public static int GetNextXp(int currentLevel)
		{
			int levelFactor = Realm.GetSetting<int>("levelfactor");
			return levelFactor * (int)Math.Pow((currentLevel + 1), 2) + levelFactor * (currentLevel + 1);
		}
	}
}
