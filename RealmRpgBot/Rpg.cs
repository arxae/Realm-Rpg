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

		/// <summary>
		/// Calculates the xp gained for a kill
		/// </summary>
		/// <param name="sourceLevel">The level of the (N)PC that will get the XP</param>
		/// <param name="targetLevel">The level of the target that has been killed</param>
		/// <returns>Fraction of how much xp should be earned</returns>
		public static double GetGainedXpModifier(int sourceLevel, int targetLevel)
		{
			int minRange = sourceLevel - 5;
			int maxRange = sourceLevel + 5;

			if (targetLevel <= minRange) return Realm.GetSetting<double>("settings/low_level_xp_penalty");
			if (targetLevel >= maxRange) return Realm.GetSetting<double>("setting/high_level_xp_bonus");

			return 1;
		}

		/// <summary>
		/// Calculates mana for a given level and int attribute
		/// </summary>
		/// <param name="level">The level of the (N)PC</param>
		/// <param name="intelligence">Intelligence attribute of the (N)PC</param>
		/// <returns>Max mana</returns>
		public static int GetMaxMana(int level, int intelligence)
		{
			var baseMana = Realm.GetSetting<int>("base_mana");
			var manaMult = Realm.GetSetting<float>("mana_int_mult");

			return (int)Math.Round(baseMana + level + (intelligence * manaMult));
		}
	}
}
