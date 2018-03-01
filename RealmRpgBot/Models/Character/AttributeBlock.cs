namespace RealmRpgBot.Models.Character
{
	public class AttributeBlock
	{
		public int Strength { get; set; }
		public int Agility { get; set; }
		public int Stamina { get; set; }
		public int Intelligence { get; set; }
		public int Wisdom { get; set; }

		public AttributeBlock() { }
		public AttributeBlock(int initializeScore)
		{
			Strength = initializeScore;
			Agility = initializeScore;
			Stamina = initializeScore;
			Intelligence = initializeScore;
			Wisdom = initializeScore;
		}

		public static AttributeBlock operator +(AttributeBlock a, AttributeBlock b)
		{
			return new AttributeBlock
			{
				Strength = a.Strength + b.Strength,
				Agility = a.Agility + b.Agility,
				Stamina = a.Stamina + b.Stamina,
				Intelligence = a.Intelligence + b.Intelligence,
				Wisdom = a.Wisdom + b.Wisdom
			};
		}

		public static AttributeBlock operator -(AttributeBlock a, AttributeBlock b)
		{
			return new AttributeBlock
			{
				Strength = a.Strength - b.Strength,
				Agility = a.Agility - b.Agility,
				Stamina = a.Stamina - b.Stamina,
				Intelligence = a.Intelligence - b.Intelligence,
				Wisdom = a.Wisdom - b.Wisdom
			};
		}
	}
}
