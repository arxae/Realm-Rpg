namespace RealmRpgBot.Models
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
		public List<Building> Buildings { get; set; }

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
			if (Buildings.Count > 0)
			{
				builder.AddField("Buildings", string.Join(", ", Buildings.Select(b => b.Name)));
			}

			// Exits
			builder.AddField("Exits", string.Join(", ", LocationConnections));

			builder.WithDescription(desc.ToString());

			return builder.Build();
		}
	}
}
