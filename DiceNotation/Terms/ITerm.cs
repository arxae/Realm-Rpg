namespace DiceNotation.Terms
{
	using Random;

	/// <summary>
	/// Interface for an evaluatable term of a dice expression.
	/// </summary>
	public interface ITerm
	{
		/// <summary>
		/// Evaluates the term and returns the result.
		/// </summary>
		/// <param name="rng">The rng to use.</param>
		/// <returns>The result of evaluating the term.</returns>
		int GetResult(IRandom rng);
	}
}