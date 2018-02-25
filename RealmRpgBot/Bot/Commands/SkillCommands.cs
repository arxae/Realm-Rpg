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

				// Check if player has that skill
				var playerSkill = player.Skills
					.FirstOrDefault(ps => ps.Id == skill.Id);

				if (playerSkill == null)
				{
					await c.RespondAsync($"{c.User.Mention} tries and tries, but nothing happens");
					await c.RejectMessage();
					return;
				}

				// Figure out what the target is
				// <@83703349525872640> -> User
				// <#402615348592902144> -> Channel
				// <@&400452230781861898> -> Role
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
			}
		}
	}
}
