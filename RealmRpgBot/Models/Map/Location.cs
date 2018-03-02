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
		public List<string> LocationConnections { get; set; }
		public List<Perceptable> Perceptables { get; set; }
		public List<LocationInventoryItem> LocationInventory { get; set; }
		public List<Building> Buildings { get; set; }
		public List<string> Resources { get; set; }
		public List<string> Encounters { get; set; }

		public Location()
		{
			// TODO: Lazy initialization
			LocationConnections = new List<string>();
			Perceptables = new List<Perceptable>();
			LocationInventory = new List<LocationInventoryItem>();
			Buildings = new List<Building>();
		}

		public DiscordEmbed GetLocationEmbed()
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
			builder.AddField("Exits", string.Join(", ", LocationConnections));

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
