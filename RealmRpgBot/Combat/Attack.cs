namespace RealmRpgBot.Combat
{
	public class Attack
	{
		public const int INFINITE = -1;

		public string Name { get; set; }
		public int DamageAmount { get; set; }
		public int Duration { get; set; }
		public DamageTypes DamageType { get; set; }

		Serilog.ILogger log;

		public Attack(string name, int amount, DamageTypes dmgType, int duration)
		{
			Name = name;
			DamageAmount = amount < 0
				? 0
				: amount;
			DamageType = dmgType;
			Duration = duration;

			log = Serilog.Log.ForContext<Attack>();
		}

		public void Execute(IBattleParticipant target)
		{
			target.HpCurrent -= DamageAmount;

			if (Duration != 0)
			{
				Duration = (Duration == INFINITE) ? INFINITE : Duration - 1;
			}

			log.Debug($"Hit {target.Name} for {DamageAmount} damage ({target.HpCurrent}/{target.HpMax})");
		}

		public enum DamageTypes
		{
			Physical
		}
	}
}
