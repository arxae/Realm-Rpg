namespace RealmRpgBot.Combat
{
	using System;

	using System.Collections.Generic;

	public class DamagePerformer
	{
		public IBattleParticipant Source { get; set; }
		public IBattleParticipant Target { get; set; }

		List<Attack> _attacks;

		public DamagePerformer(IBattleParticipant src, IBattleParticipant trg)
		{
			Source = src;
			Target = trg;
			_attacks = new List<Attack>();
		}

		public void AddAttack(Attack dmg)
		{
			if (dmg.Duration == 0)
			{
				throw new ArgumentException($"Tried to add effect {dmg.Name} to an EffectTrigger, but it has duration 0!", nameof(dmg));
			}

			_attacks.Add(dmg);
		}

		public void TriggerDamage()
		{
			foreach (var att in _attacks)
			{
				if (att.Duration == 0) continue;
				att.Execute(Target);
			}

			_attacks.RemoveAll(att => att.Duration == 0);
		}
	}
}
