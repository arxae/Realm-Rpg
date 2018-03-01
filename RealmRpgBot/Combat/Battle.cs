namespace RealmRpgBot.Combat
{
	public class Battle
	{
		public DamagePerformer A { get; private set; }
		public DamagePerformer B { get; private set; }
		public int Round { get; set; }

		public CombatResult AttackerResult { get; set; }
		public CombatResult DefenderResult { get; set; }

		Serilog.ILogger log;

		public Battle(IBattleParticipant att, IBattleParticipant def)
		{
			A = new DamagePerformer(att, def);
			B = new DamagePerformer(def, att);

			Round = 0;

			log = Serilog.Log.ForContext<Battle>();
		}

		public void DoCombat()
		{
			while (A.Source.HpCurrent > 0 && B.Source.HpCurrent > 0)
			{
				Round++;
				DoRound();
			}

			AttackerResult = A.Source.HpCurrent > 0 ? CombatResult.Win : CombatResult.Loss;
			DefenderResult = B.Source.HpCurrent > 0 ? CombatResult.Win : CombatResult.Loss;

			if (AttackerResult == DefenderResult)
			{
				AttackerResult = CombatResult.Tie;
				DefenderResult = CombatResult.Tie;
			}

			var victorName = A.Source.HpCurrent > 0 ? A.Source.Name : B.Source.Name;
			log.Debug($"Combat ended after {Round} round(s). Victor: {victorName}");
		}

		void DoRound()
		{
			// TODO: Determine attacks
			// For now, do random damage based on str
			// A
			var a_dmg = A.Source.Attributes.Strength + DiceNotation.SingletonRandom.Instance.Next(5);
			A.AddAttack(new Attack("A Attack", a_dmg, Attack.DamageTypes.Physical, 1));

			// B
			var b_dmg = B.Source.Attributes.Strength + DiceNotation.SingletonRandom.Instance.Next(5);
			B.AddAttack(new Attack("B Attack", b_dmg, Attack.DamageTypes.Physical, 1));

			A.TriggerDamage();
			B.TriggerDamage();
		}

		public enum CombatResult
		{
			Win,
			Loss,
			Tie
		}
	}
}
