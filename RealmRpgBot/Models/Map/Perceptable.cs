namespace RealmRpgBot.Models.Map
{
	/// <summary>
	/// Definition for items/events that can be found on a certain location. See <seealso cref="RealmRpgBot.Models.Inventory.Item"/> for the actual item definition
	/// </summary>
	public class Perceptable
	{
		public string DocId { get; set; }
		public int Difficulty { get; set; }
		public int Count { get; set; }
		public int MaxPerceptable { get; set; }
		public PerceptableType Type { get; set; }

		public override string ToString() => $"{DocId} - {Difficulty} - {Type.ToString()}";

		public enum PerceptableType
		{
			Item,
			HiddenExit,
			Event
		}
	}
}
