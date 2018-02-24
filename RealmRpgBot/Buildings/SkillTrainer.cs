namespace RealmRpgBot.Buildings
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using DSharpPlus.Interactivity;

	using Models;

	public class SkillTrainer : IBuilding
	{
		public async Task EnterBuilding(CommandContext c, Building building)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync(Constants.MSG_NOT_REGISTERED);
					await c.RejectMessage();
					return;
				}

				var skillMenuBuilder = new DiscordEmbedBuilder()
					.WithTitle(building.Name);

				var description = new System.Text.StringBuilder();
				description.AppendLine($"*\"{building.Parameters["WelcomeMessage"]}\"*");

				// Get skills
				var skills = await session.LoadAsync<Skill>(building.Parameters.Where(p => p.Value.StartsWith("skills/")).Select(p => p.Value));

				foreach (var skill in skills.Values)
				{
					if (skill.TrainingCosts == null) continue;

					var descriptionLines = skill.Description.Split('\n');
					string descriptionText = descriptionLines[0];
					if(descriptionLines.Length > 1)
					{
						descriptionText += "...";
					}

					description.AppendLine($"{skill.ReactionIcon} {skill.DisplayName} (Cost: {skill.TrainingCosts[0]}) - *{descriptionText}*");
				}

				skillMenuBuilder.WithDescription(description.ToString());

				if (player.SkillPoints < 1)
				{
					skillMenuBuilder.WithFooter("Only showing buttons for skills you can purchase. For full descriptions, use the *.info skill <skillname>* command");
				}

				var msg = await c.RespondAsync(embed: skillMenuBuilder.Build());

				if (player.SkillPoints < 1) return;

				foreach (var skill in skills.Values)
				{
					if (skill.TrainingCosts == null) continue;
					await msg.CreateReactionAsync(DiscordEmoji.FromName(c.Client, skill.ReactionIcon));
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

				var selectedSkill = skills.FirstOrDefault(s => s.Value.ReactionIcon.Equals(responseName));

				var skillEntry = player.Skills.FirstOrDefault(ls => ls.Id.Equals(selectedSkill.Key));
				if (skillEntry == null)
				{
					player.Skills.Add(new TrainedSkill { Id = selectedSkill.Key, Rank = 1 });
					await c.RespondAsync($"{c.User.Mention} learned {selectedSkill.Value.DisplayName}");
				}
				else
				{
					skillEntry.Rank += 1;
					await c.RespondAsync($"{c.User.Mention} {selectedSkill.Value.DisplayName} has increased to {skillEntry.Rank}");
				}

				// TODO: Get correct level cost
				player.SkillPoints -= selectedSkill.Value.TrainingCosts[0];

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
