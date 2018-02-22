﻿namespace RealmRpgBot.Models
{
	using System.Collections.Generic;

    public class Building
    {
        public string Name { get; set; }
		public string BuildingImpl { get; set; }
		public List<string> Actions { get; set; }
		public Dictionary<string, string> Parameters { get; set; }
    }
}
