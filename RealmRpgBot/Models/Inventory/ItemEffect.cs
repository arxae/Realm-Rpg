namespace RealmRpgBot.Models.Inventory
{
	using System.Collections.Generic;

	public class ItemEffect
	{
		public ItemEffects Effect { get; set; }
		public Dictionary<string, object> Parameters { get; set; }

		public T GetParameter<T>(string name)
		{
			if (Parameters.ContainsKey(name))
			{
				return (T) System.Convert.ChangeType(Parameters[name], typeof(T));
			}

			return default(T);
		}

		public enum ItemEffects
		{
			Restore
		}
	}
}