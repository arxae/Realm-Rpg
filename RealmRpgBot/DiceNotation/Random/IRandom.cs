namespace RealmRpgBot.DiceNotation.Random
{
	/// <summary>
	/// Interface for pseudo-random number generators to implement.
	/// </summary>
	public interface IRandom
	{
		/// <summary>
		/// Gets the next pseudo-random integer between 0 and the specified maxValue, inclusive.
		/// </summary>
		/// <param name="maxValue">Inclusive maximum result</param>
		/// <returns>Returns a pseudo-random integer between 0 and the specified maxValue, inclusive</returns>
		int Next(int maxValue);

		/// <summary>
		/// Gets the next pseudo-random integer between the specified minValue and maxValue, inclusive.
		/// </summary>
		/// <param name="minValue">Inclusive minimum result.</param>
		/// <param name="maxValue">Inclusive maximum result.</param>
		/// <returns>
		/// Returns a pseudo-random integer between the specified minValue and maxValue inclusive.
		/// </returns>
		int Next(int minValue, int maxValue);
	}
}