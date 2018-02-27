using System;

namespace RealmRpgBot.Bot.Commands
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using Raven.Client.Documents;

	[Group("skill"),
		Description("Skill related commands"),
		RequireRoles(RoleCheckMode.Any, "Realm Admin", "Realm Player")]
	public class SkillCommands : RpgCommandBase
	{
		// Format: .skill use <skillname> on <othername>
		[Command("use"), Description("Use a skill")]
		public async Task UseSkill(CommandContext c,
			[Description("Use a skill"), RemainingText] string input)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				// Check if player exists
				var player = await session
					.LoadAsync<Models.Player>(c.User.Id.ToString());

				if (player == null)
				{
					await c.RespondAsync($"{c.User.Mention}, {Constants.MSG_NOT_REGISTERED}");
					await c.RejectMessage();
					return;
				}

				if (player.IsIdle == false)
				{
					await c.RespondAsync($"{c.User.Mention}, {Constants.MSG_PLAYER_NOT_IDLE}: {player.CurrentActionDisplay}");
					await c.RejectMessage();
					return;
				}

				// Check if player has any skills
				if (player.Skills == null || player.Skills.Count == 0)
				{
					await c.RespondAsync($"{c.User.Mention} has no skills, poor you");
					await c.RejectMessage();
					return;
				}

				// Find skill to execute
				var cmdSplit = input.SplitCaseIgnore(new[] { " on " }, true);

				string skillName;
				if (cmdSplit.Count == 1)
				{
					skillName = input.EndsWith(" on")
						? input.Substring(0, input.LastIndexOf(" on", StringComparison.OrdinalIgnoreCase))
						: input;
				}
				else
				{
					skillName = cmdSplit[0];
				}

				var skill = await session.Query<Models.Skill>()
					.FirstOrDefaultAsync(s => s.DisplayName.Equals(skillName, StringComparison.OrdinalIgnoreCase));

				if (skill.IsActivatable == false)
				{
					await c.RespondAsync($"{c.User.Mention}, this skill cannot be activated");
					await c.RejectMessage();
					await c.ConfirmMessage();
				}

				// Check if player has that skill
				var playerSkill = player.Skills
					.FirstOrDefault(ps => ps.Id == skill.Id);

				if (playerSkill == null)
				{
					await c.RespondAsync($"{c.User.Mention} tries and tries, but nothing happens");
					await c.RejectMessage();
					return;
				}

				if (playerSkill.CooldownUntil > DateTime.Now)
				{
					var remaining = playerSkill.CooldownUntil - DateTime.Now;
					await c.RespondAsync($"{c.User.Mention}. That skill is still on cooldown ({(int)remaining.TotalSeconds}s more)");
					await c.ConfirmMessage();

					return;
				}

				// Figure out what the target is
				object target = null;
				if (cmdSplit.Count > 1)
				{
					target = cmdSplit[1];
				}

				// Execute skill on target
				var sType = Realm.GetSkillImplementation(skill.SkillImpl);
				if (sType == null)
				{
					await c.RespondAsync($"An error occured. Contact one of the admins (Error_SkillWitouthSkillImpl:{skill.Id})");
					await c.RejectMessage();
					return;
				}

				var sImpl = (Skills.ISkill)Activator.CreateInstance(sType);
				await sImpl.ExecuteSkill(c, skill, playerSkill, player, target);

				if (skill.CooldownRanks?.Count > 0 && sImpl.DoCooldown)
				{
					if (playerSkill.Rank > skill.CooldownRanks.Count)
					{
						playerSkill.CooldownUntil = DateTime.Now.AddSeconds(skill.CooldownRanks[skill.CooldownRanks.Count - 1]);
					}
					else
					{
						playerSkill.CooldownUntil = DateTime.Now.AddSeconds(skill.CooldownRanks[playerSkill.Rank - 1]);
					}
				}

				if (session.Advanced.HasChanges)
				{
					await session.SaveChangesAsync();
				}
			}
		}
	}
}
