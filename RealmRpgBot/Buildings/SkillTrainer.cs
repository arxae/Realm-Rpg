namespace RealmRpgBot.Buildings
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using DSharpPlus.Interactivity;

	using Models.Character;
	using Models.Map;

	public class SkillTrainer : IBuilding
	{
		public async Task EnterBuilding(CommandContext c, Building building)
		{
			var log = Serilog.Log.ForContext<SkillTrainer>();
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RejectMessage(Realm.GetMessage("not_registered"));
					return;
				}

				var skillMenuBuilder = new DiscordEmbedBuilder()
					.WithTitle(building.Name);

				var description = new System.Text.StringBuilder();
				description.AppendLine($"*\"{building.WelcomeMessage}\"*");

				// Get skills
				var skillsDict = await session.LoadAsync<Skill>(building.Parameters.Where(p => p.Value.StartsWith("skills/")).Select(p => p.Value));
				var skills = skillsDict.Where(kv => kv.Value != null).Select(s => s.Value).ToList();
				
				foreach (var skill in skills)
				{
					if (skill.TrainingCosts == null)
					{
						log.Error("No trainingcosts defined for {skillname} ({skillid}).", skill.DisplayName, skill.Id);
						return;
					}

					var descriptionLines = skill.Description.Split('\n');
					string descriptionText = descriptionLines[0];
					if (descriptionLines.Length > 0) descriptionText += "...";

					// Get the  player rank
					int playerSkillRank = 0;
					var playerSkill = player.Skills.FirstOrDefault(ps => ps.Id == skill.Id);
					if (playerSkill != null)
					{
						playerSkillRank = playerSkill.Rank;
					}

					// Check if max rank
					if (skill.TrainingCosts.Count == playerSkillRank)
					{
						description.AppendLine($"{skill.ReactionIcon} {skill.DisplayName} (Max skill) - *{descriptionText}*");
						continue;
					}

					// Get next cost
					int rankCost = skill.TrainingCosts[playerSkillRank];
					description.AppendLine($"{skill.ReactionIcon} {skill.DisplayName} (Cost: {rankCost}) - *{descriptionText}*");
				}

				skillMenuBuilder.WithDescription(description.ToString());

				if (player.SkillPoints < 1)
				{
					skillMenuBuilder.WithFooter("You do not have any skillpoints");
				}

				var msg = await c.RespondAsync(embed: skillMenuBuilder.Build());

				if (player.SkillPoints < 1) return;

				foreach (var skill in skills)
				{
					if (skill.TrainingCosts == null) continue;

					// Get the  player rank
					int playerSkillRank = 0;
					var playerSkill = player.Skills.FirstOrDefault(ps => ps.Id == skill.Id);
					if (playerSkill != null)
					{
						playerSkillRank = playerSkill.Rank;
					}

					// Check if max rank
					if (skill.TrainingCosts.Count != playerSkillRank)
					{
						await msg.CreateReactionAsync(DiscordEmoji.FromName(c.Client, skill.ReactionIcon));
					}
				}

				var interact = c.Client.GetInteractivity();
				var response = await interact.WaitForMessageReactionAsync(msg, c.User, System.TimeSpan.FromSeconds(10));

				if (response == null)
				{
					await c.RejectMessage();
					await msg.DeleteAsync();
					return;
				}

				await msg.DeleteAllReactionsAsync();
				var responseName = response.Emoji.GetDiscordName().ToLower();
				var selectedSkill = skills.FirstOrDefault(s => s.ReactionIcon.Equals(responseName));
				var skillEntry = player.Skills.FirstOrDefault(ls => ls.Id.Equals(selectedSkill?.Id));
				
				var currentPlayerSkill = player.Skills.FirstOrDefault(ps => ps.Id == selectedSkill.Id);
				int nextRankCost = currentPlayerSkill != null 
					? selectedSkill.TrainingCosts[currentPlayerSkill.Rank] 
					: selectedSkill.TrainingCosts[0];

				if (nextRankCost > player.SkillPoints)
				{
					await c.RejectMessage(string.Format(Realm.GetMessage("next_skill_not_enough_skillpts"), selectedSkill.DisplayName));
					return;
				}

				if (skillEntry == null)
				{
					player.Skills.Add(new TrainedSkill(selectedSkill.Id, 1));
					await c.RespondAsync($"{c.User.Mention} learned {selectedSkill.DisplayName}");
				}
				else
				{
					skillEntry.Rank += 1;
					await c.RespondAsync($"{c.User.Mention} {selectedSkill.DisplayName} has increased to {skillEntry.Rank}");
				}

				player.SkillPoints -= nextRankCost;

				if (session.Advanced.HasChanged(player))
				{
					await session.SaveChangesAsync();
				}

				await msg.DeleteAsync();
				await c.ConfirmMessage();
			}
		}
	}
}
