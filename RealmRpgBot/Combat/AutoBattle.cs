namespace RealmRpgBot.Combat
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	/// <summary>
	/// Battle system where the moves are automatically determined and executed
	/// </summary>
	public class AutoBattle
	{
		public int Round { get; set; }

		public IBattleParticipant Attacker { get; set; }
		public List<Attack> AttackerAttacks { get; set; }

		public IBattleParticipant Defender { get; set; }
		public List<Attack> DefenderAttacks { get; set; }

		public CombatOutcome Outcome { get; set; }

		public List<string> CombatLog { get; private set; }

		readonly string CombatId;
		readonly Serilog.ILogger log;
		readonly CommandContext cmdCtx;

		public AutoBattle(IBattleParticipant attacker, IBattleParticipant defender, CommandContext c)
		{
			log = Serilog.Log.ForContext<AutoBattle>();

			Attacker = attacker;
			Defender = defender;
			cmdCtx = c;

			Round = 0;
			CombatLog = new List<string>();
			AttackerAttacks = new List<Attack>();
			DefenderAttacks = new List<Attack>();

			CombatId = Guid.NewGuid().ToString().Substring(0, 8);
		}

		public async Task StartCombatAsync()
		{
			log.Debug("[ac:{id}] Start combat {a} vs {b}", CombatId, Attacker.Name, Defender.Name);
			log.Debug("{name} bonus (a/d) {a}/{d}", Attacker.Name, Attacker.GetEquipmentBonusses().AttackBonus, Attacker.GetEquipmentBonusses().DefenceBonus);
			log.Debug("{name} bonus (a/d) {a}/{d}", Defender.Name, Defender.GetEquipmentBonusses().AttackBonus, Defender.GetEquipmentBonusses().DefenceBonus);

			while (Attacker.HpCurrent > 0 && Defender.HpCurrent > 0)
			{
				Round++;
				await DoRoundAsync();
			}

			if (Attacker.HpCurrent <= 0 && Defender.HpCurrent <= 0) { Outcome = CombatOutcome.Tie; }
			else if (Attacker.HpCurrent > 0 && Defender.HpCurrent <= 0) { Outcome = CombatOutcome.Attacker; }
			else if (Attacker.HpCurrent <= 0 && Defender.HpCurrent > 0) { Outcome = CombatOutcome.Defender; }

			await StoreCombatLogAsync();

			log.Debug("[ac:{id}] Combat ended", CombatId);
		}

		async Task DoRoundAsync()
		{
			await Task.Run(() =>
			{

				CombatLog.Add($"Begin round {Round}");

				// Issue attacks
				// TODO: Players: Allow to set when to heal etc..
				// TODO: Monsters: Heal when < x% hp (have setting to be able to heal in EncounterTemplate)
				AttackerAttacks.Add(new Attack($"{Attacker.Name} sword strike",
					Attacker.GetEquipmentBonusses().AttackBonus + DiceNotation.Dice.Roll("1d6"),
					Attack.DamageTypes.Physical, 1));

				DefenderAttacks.Add(new Attack($"{Defender.Name} claw strike",
					Defender.GetEquipmentBonusses().AttackBonus + DiceNotation.Dice.Roll("1d6"),
					Attack.DamageTypes.Physical, 1));

				// Execute all attacks with duration > 1

				foreach (var a in AttackerAttacks)
				{
					if (a.Duration == 0) continue;
					a.Execute(Defender);
					CombatLog.Add($"   {Defender.Name} got hit by {a.Name} for {a.DamageAmount} {a.DamageType} damage. {Defender.HpCurrent}hp remaining");
				}

				foreach (var a in DefenderAttacks)
				{
					if (a.Duration == 0) continue;
					a.Execute(Attacker);
					CombatLog.Add($"   {Attacker.Name} got hit by {a.Name} for {a.DamageAmount} {a.DamageType} damage. {Attacker.HpCurrent}hp remaining");
				}

				// Remove all attacks with duration 0
				AttackerAttacks.RemoveAll(a => a.Duration == 0);
				DefenderAttacks.RemoveAll(a => a.Duration == 0);
			});
		}

		public async Task<CombatResult> GetCombatResultAsync()
		{
			return await Task.Run(() =>
			{
				string msg;

				switch (Outcome)
				{
					case CombatOutcome.Undetermined:
						msg = "You lost track of each other in the middle of the battle. After the dust settles, the enemy is gone";
						break;
					case CombatOutcome.Attacker:
						msg = $"{cmdCtx.User.Mention} has won the battle with {Attacker.HpCurrent}hp left.";
						break;
					case CombatOutcome.Defender:
						msg = $"{cmdCtx.User.Mention} has been knocked out. It had {Defender.HpCurrent}hp left.";
						break;
					case CombatOutcome.Tie:
						msg = "During the scuffle, you knock eachother out";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				return new CombatResult(Outcome, msg);
			});
		}

		async Task StoreCombatLogAsync()
		{
			string winnerName;
			string loserName;

			switch (Outcome)
			{
				case CombatOutcome.Attacker:
					winnerName = Attacker.Name;
					loserName = Defender.Name;
					break;
				case CombatOutcome.Defender:
					winnerName = Defender.Name;
					loserName = Attacker.Name;
					break;
				default:
					winnerName = "None";
					loserName = "None";
					break;
			}

			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var entry = await session.LoadAsync<Models.CombatLog>("combatlogs/" + cmdCtx.User.Id);

				if (entry == null)
				{
					await session.StoreAsync(new Models.CombatLog
					{
						Id = "combatlogs/" + cmdCtx.User.Id,
						Lines = new List<string>(CombatLog),
						Loser = loserName,
						Outcome = Outcome,
						Rounds = Round,
						Timestamp = DateTime.Now,
						Winner = winnerName
					});

					entry = await session.LoadAsync<Models.CombatLog>("combatlogs/" + cmdCtx.User.Id);
				}
				else
				{
					entry.Lines.Clear();
					entry.Lines.AddRange(CombatLog);
					entry.Loser = loserName;
					entry.Outcome = Outcome;
					entry.Rounds = Round;
					entry.Timestamp = DateTime.Now;
					entry.Winner = winnerName;
				}

				var metadata = session.Advanced.GetMetadataFor(entry);
				metadata[Raven.Client.Constants.Documents.Metadata.Expires] = DateTime.UtcNow.AddMinutes(30).ToString("o");

				await session.SaveChangesAsync();
			}
		}

		public class CombatResult
		{
			public string Message { get; set; }
			public CombatOutcome Outcome { get; set; }

			public CombatResult(CombatOutcome outc, string msg)
			{
				Message = msg;
				Outcome = outc;
			}

			public override string ToString() => Message;
		}
	}
}