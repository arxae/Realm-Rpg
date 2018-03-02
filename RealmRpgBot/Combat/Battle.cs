namespace RealmRpgBot.Combat
{
	using System.Collections.Generic;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	public class Battle
	{
		public DamagePerformer A { get; private set; }
		public DamagePerformer B { get; private set; }
		public int Round { get; set; }

		public CombatResult AttackerResult { get; set; }
		public CombatResult DefenderResult { get; set; }

		public List<string> CombatLog { get; set; }

		Serilog.ILogger log;
		CommandContext cmdCtx;

		public Battle(IBattleParticipant att, IBattleParticipant def, CommandContext c)
		{
			A = new DamagePerformer(att, def);
			B = new DamagePerformer(def, att);

			Round = 0;
			CombatLog = new List<string>();

			log = Serilog.Log.ForContext<Battle>();
			cmdCtx = c;
		}

		public async Task DoCombatAsync()
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

			await StoreCombatLogAsync();
		}

		void DoRound()
		{
			// TODO: Determine attacks
			// For now, do random damage based on str
			// A
			var a_rng = DiceNotation.Dice.Roll("1d6");
			var a_dmg = A.Source.Attributes.Strength + a_rng;
			A.AddAttack(new Attack("A Attack", a_dmg, Attack.DamageTypes.Physical, 1));

			// B
			var b_rng = DiceNotation.Dice.Roll("1d6");
			var b_dmg = B.Source.Attributes.Strength + b_rng;
			B.AddAttack(new Attack("B Attack", b_dmg, Attack.DamageTypes.Physical, 1));

			A.TriggerDamage();
			B.TriggerDamage();

			// TODO: Propper combat log
			CombatLog.Add($"Round {Round}:");
			CombatLog.Add($"   {A.Source.Name} hits for {a_dmg} physical damage ({B.Source.Name} has {B.Source.HpCurrent}/{B.Source.HpMax} remaining)");
			CombatLog.Add($"   {B.Source.Name} hits for {b_dmg} physical damage ({A.Source.Name} has {A.Source.HpCurrent}/{A.Source.HpMax} remaining)");
		}

		async Task StoreCombatLogAsync()
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var entry = await session.LoadAsync<Models.CombatLog>("combatlogs/" + cmdCtx.User.Id);

				if (entry == null)
				{
					await session.StoreAsync(new Models.CombatLog
					{
						Id = "combatlogs/" + cmdCtx.User.Id,
						Lines = new List<string>(CombatLog)
					});
				}
				else
				{
					entry.Lines.Clear();
					entry.Lines.AddRange(CombatLog);
				}

				await session.SaveChangesAsync();
			}
		}

		public enum CombatResult
		{
			Win,
			Loss,
			Tie
		}
	}
}
