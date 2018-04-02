﻿using Raven.Client.Documents.Attachments;
using Raven.Client.Documents.Operations.Attachments;

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
		public List<string> Events { get; set; }
		public EncounterTypes EncounterType { get; set; }
		public int XpReward { get; set; }

		public int GetActualXpReward(int playerLevel, int enemyLevel)
		{
			return (int)System.Math.Round(XpReward * Rpg.GetGainedXpModifier(playerLevel, enemyLevel));
		}

		public async Task DoEncounter(IAsyncDocumentSession session, CommandContext cmdCtx, Player player)
		{
			if (Templates.Count == 0 && Events.Count == 0)
			{
				throw new System.InvalidOperationException($"No encounter tempaltes or events have been defined for {Id}");
			}

			if (Templates.Count == 0 && Events.Count > 0)
			{
				await DoEventEncounter(session, cmdCtx, player);
				return;
			}

			if (Templates.Count > 0 && Events.Count == 0)
			{
				await DoBattleEncounter(session, cmdCtx, player);
				return;
			}

			// 50/50 chance for either
			if (Rng.Instance.Next(0, 100) > 50)
			{
				await DoEventEncounter(session, cmdCtx, player);
			}
			else
			{
				await DoBattleEncounter(session, cmdCtx, player);
			}
		}

		public async Task DoEventEncounter(IAsyncDocumentSession session, CommandContext cmdCtx, Player player)
		{
			var events = (await session.LoadAsync<Event>(Events)).Values.ToList();
			if (events.Count == 0)
			{
				await cmdCtx.ConfirmMessage("Nothing to see here");
				return;
			}

			var evt = events.GetRandomEntry();
			evt.Stages.ForEach(async stage =>
			{
				var stageScript = await Db.DocStore.Operations.SendAsync(new GetAttachmentOperation(evt.Id, stage.Script, AttachmentType.Document, null));

				if (stageScript == null)
				{
					Serilog.Log.ForContext<Encounter>().Error($"{evt.Id}-{stage.StageName} is missing an action script.");
					await cmdCtx.RejectMessage($"{evt.Id}-{stage.StageName} is missing an action script. Contact one of the admins (Error_EventStageScriptNotFound)");
					return;
				}

				string script = await new System.IO.StreamReader(stageScript.Stream).ReadToEndAsync();
				await Script.ScriptManager.RunDiscordScriptAsync($"{evt.Id}-{stage.StageName}", script, cmdCtx, player, null);
			});
		}

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
				await cmdCtx.ConfirmMessage("Nothing to see here");
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
			player.SetIdleAction("Resting");

			await cmdCtx.RespondAsync(embed: encounterEmbed.Build());
		}



		public enum EncounterTypes
		{
			None,
			Battle,
			Event
		}
	}
}
