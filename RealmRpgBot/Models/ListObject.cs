namespace RealmRpgBot.Models
{
	using System.Collections.Generic;
	using System.Linq;

	public class ListObject
	{
		public string Id { get; set; }
		public List<string> Items { get; set; }

		public bool ContainsItem(string item) => Items.Contains(item, System.StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets the correct-cased item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public string GetCorrectCaseItem(string item)
		{
			int index = Items.FindIndex(i => i.Equals(item, System.StringComparison.OrdinalIgnoreCase));
			return Items[index];
		}
	}
}
