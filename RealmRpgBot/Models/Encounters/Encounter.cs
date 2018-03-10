namespace RealmRpgBot.Models.Encounters
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using Raven.Client.Documents.Session;

	using Character;
	using Combat;

	public class Encounter
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public List<string> Templates { get; set; }
		public EncounterTypes EncounterType { get; set; }
		public int XpReward { get; set; }

		public int GetActualXpReward(int playerLevel, int enemyLevel) => XpReward / 100 * Rpg.GetGainedXpModifier(playerLevel, enemyLevel);

		/// <summary>
		/// Do automatic battle encounter
		/// </summary>
		/// <param name="session">Database session. One should be open anyway to get encounter info</param>
		/// <param name="cmdCtx">Discord command context</param>
		/// <param name="player">Player doing the encounter</param>
		/// <returns></returns>
		public async Task DoBattleEncounter(IAsyncDocumentSession session, CommandContext cmdCtx, Player player)
		{
			var templates = (await session.LoadAsync<EncounterTemplate>(Templates)).Values
							.Where(t => t.LevelRangeMin >= player.Level && player.Level <= t.LevelRangeMin || t.AdjustToPlayerLevel)
							.ToList();

			if (templates.Count == 0)
			{
				await cmdCtx.RespondAsync("Nothing to see here");
				await cmdCtx.ConfirmMessage();
				return;
			}

			var template = templates.GetRandomEntry();

			var enemy = new Enemy(template, player.Level);

			var encounterEmbed = new DiscordEmbedBuilder()
				.WithTitle($"Encounter with {enemy.Name}");

			var body = new System.Text.StringBuilder();
			body.AppendLine($"{cmdCtx.User.Mention} has encountered a lvl{enemy.Level} {enemy.Name} with {enemy.HpCurrent}hp.");

			var combat = new AutoBattle(player, enemy, cmdCtx);
			await combat.StartCombatAsync();

			var result = await combat.GetCombatResultAsync();

			string xpMsg = string.Empty;
			switch (result.Outcome)
			{
				case CombatOutcome.Attacker:
					{
						var xpGain = GetActualXpReward(player.Level, enemy.Level);
						await player.AddXpAsync(xpGain, cmdCtx);
						xpMsg = $"Gained {xpGain}xp";
						break;
					}
				case CombatOutcome.Defender:
					{
						var xpLost = await player.SetFaintedAsync();
						xpMsg = $"Lost {xpLost}xp.";
						break;
					}
				case CombatOutcome.Tie:
					{
						var xpGain = GetActualXpReward(player.Level, enemy.Level);
						await player.AddXpAsync(xpGain, cmdCtx);
						var xpLost = await player.SetFaintedAsync();
						xpMsg = $"Gained {xpGain}xp, but lost {xpLost}xp.";
						break;
					}
			}

			encounterEmbed.WithFooter(xpMsg);

			body.AppendLine();
			body.AppendLine("*Last lines of Combat Log*");
			body.AppendLine("*...*");

			foreach (var line in combat.CombatLog.Skip(System.Math.Max(0, combat.CombatLog.Count - 3)))
			{
				body.AppendLine(line);
			}

			body.AppendLine();
			body.AppendLine(result.Message);

			encounterEmbed.WithDescription(body.ToString());

			await player.SetActionAsync(Constants.ACTION_REST, "Recovering from combat", System.TimeSpan.FromMinutes(1));

			await cmdCtx.RespondAsync(embed: encounterEmbed.Build());
		}

		public enum EncounterTypes
		{
			Enemy
		}
	}
}
