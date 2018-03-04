namespace RealmRpgBot.Combat
{
	using System.Collections.Generic;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	public class Battle
	{
		public CombatPerformer A { get; private set; }
		public CombatPerformer B { get; private set; }
		public int Round { get; set; }

		public CombatResult Outcome { get; set; }
		public string VictorName { get; set; }
		public string LoserName { get; set; }

		public List<string> CombatLog { get; set; }

		Serilog.ILogger log;
		CommandContext cmdCtx;

		public Battle(IBattleParticipant att, IBattleParticipant def, CommandContext c)
		{
			A = new CombatPerformer(att, def);
			B = new CombatPerformer(def, att);

			Round = 0;
			CombatLog = new List<string>();
			Outcome = CombatResult.Undetermined;

			log = Serilog.Log.ForContext<Battle>();
			cmdCtx = c;
		}

		public async Task DoCombatAsync()
		{
			log.Debug("Start combat {a} vs {b}", A.Source.Name, B.Source.Name);

			while (A.Source.HpCurrent > 0 && B.Source.HpCurrent > 0)
			{
				if (Round >= 999) continue;

				Round++;
				DoRound();
			}

			if (A.Source.HpCurrent < 0 && B.Source.HpCurrent < 0)
			{
				Outcome = CombatResult.Tie;
				VictorName = "None";
				LoserName = "None";
			}
			else if (A.Source.HpCurrent > 0 && B.Source.HpCurrent < 0)
			{
				Outcome = CombatResult.WinA;
				VictorName = A.Source.Name;
				LoserName = B.Source.Name;
			}
			else if (A.Source.HpCurrent < 0 && B.Source.HpCurrent > 0)
			{
				Outcome = CombatResult.WinB;
				VictorName = B.Source.Name;
				LoserName = A.Source.Name;
			}
			else
			{
				VictorName = "None";
				LoserName = "None";
			}

			await StoreCombatLogAsync();

			log.Debug("End combat {a} vs {b} (outcome: {o})", A.Source.Name, B.Source.Name, Outcome);
		}

		void DoRound()
		{
			// Issue attacks
			// TODO: Determine moves
			// For now, do random damage based on str
			// A
			var a_dmg = A.Source.Attributes.Strength + DiceNotation.Dice.Roll("1d6");
			A.AddAttack(new Attack("A Attack", a_dmg, Attack.DamageTypes.Physical, 1));

			// B
			var b_dmg = B.Source.Attributes.Strength + DiceNotation.Dice.Roll("1d6");
			B.AddAttack(new Attack("B Attack", b_dmg, Attack.DamageTypes.Physical, 1));

			// TODO: Based on a stat
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
						Lines = new List<string>(CombatLog),
						Loser = LoserName,
						Result = Outcome,
						Rounds = Round,
						Timestamp = System.DateTime.Now,
						Victor = VictorName
					});
				}
				else
				{
					entry.Lines.Clear();
					entry.Lines.AddRange(CombatLog);
					entry.Loser = LoserName;
					entry.Result = Outcome;
					entry.Rounds = Round;
					entry.Timestamp = System.DateTime.Now;
					entry.Victor = VictorName;
				}

				await session.SaveChangesAsync();
			}
		}

		public enum CombatResult
		{
			Undetermined,
			WinA,
			WinB,
			Tie
		}
	}
}
