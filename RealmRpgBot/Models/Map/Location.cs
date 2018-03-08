namespace RealmRpgBot.Models.Map

{
	using System.Collections.Generic;
	using System.Linq;

	using DSharpPlus.Entities;

	public class Location
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public string SafetyRating { get; set; }
		public Dictionary<string, string> LocationConnections { get; set; }
		public Dictionary<string, string> HiddenLocationConnections { get; set; }
		public List<Perceptable> Perceptables { get; set; }
		public List<LocationInventoryItem> LocationInventory { get; set; }
		public List<Building> Buildings { get; set; }
		public List<string> Resources { get; set; }
		public List<string> Encounters { get; set; }

		public Location()
		{
			LocationConnections = new Dictionary<string, string>();
			HiddenLocationConnections = new Dictionary<string, string>();
			Perceptables = new List<Perceptable>();
			LocationInventory = new List<LocationInventoryItem>();
			Buildings = new List<Building>();
			Resources = new List<string>();
			Encounters = new List<string>();
		}

		public DiscordEmbed GetLocationEmbed(List<string> foundHiddenLocations = null)
		{
			var builder = new DiscordEmbedBuilder()
				.WithTitle(DisplayName);

			var desc = new System.Text.StringBuilder(Description);

			// Hostility Colors
			switch (SafetyRating.ToLower())
			{
				case "sanctuary": builder.WithColor(DiscordColor.Gold); break;
				case "friendly": builder.WithColor(DiscordColor.SpringGreen); break;
				case "neutral": builder.WithColor(DiscordColor.CornflowerBlue); break;
				case "caution": builder.WithColor(DiscordColor.Orange); break;
				case "dangerous": builder.WithColor(DiscordColor.IndianRed); break;
				case "hostile": builder.WithColor(DiscordColor.Red); break;
				default: builder.WithColor(DiscordColor.White); break;
			}

			// Buildings
			if (Buildings?.Count > 0)
			{
				builder.AddField("Buildings", string.Join(", ", Buildings.Select(b => b.Name)));
			}

			// Exits
			var exits = new List<string>();
			if (LocationConnections.Keys.Count > 0)
			{
				exits.AddRange(LocationConnections.Keys);
			}

			// Include hidden exits (if found)
			if (HiddenLocationConnections.Keys.Count > 0 && foundHiddenLocations != null)
			{
				var toInclude = HiddenLocationConnections
					.Where(l => foundHiddenLocations.Contains(l.Value))
					.Select(l => l.Key);
				exits.AddRange(toInclude);
			}

			builder.AddField("Exits", string.Join(", ", exits));

			// Location Inventory
			if (LocationInventory?.Count > 0)
			{
				var items = LocationInventory.Select(inv => $"{inv.DisplayName} (x{inv.Amount})");
				builder.AddField("Items", string.Join(", ", items));
			}

			builder.WithDescription(desc.ToString());

			return builder.Build();
		}
	}
}
