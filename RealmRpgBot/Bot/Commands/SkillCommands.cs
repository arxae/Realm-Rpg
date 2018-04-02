namespace RealmRpgBot.Bot.Commands
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.Entities;
	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using Raven.Client.Documents;

	using Models.Character;

	[Group("skill"),
		Description("Skill related commands"),
		RequireRoles(RoleCheckMode.Any, Constants.ROLE_PLAYER)]
	public class SkillCommands : RpgCommandBase
	{
		[Command("use")]
		public async Task UseSkill(CommandContext c,
			[Description("Skillname")] string skillName,
			[Description("Target, leave blank to target self/area")]
			DiscordMember target = null)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(c.User.Id.ToString());

				if (player.IsIdle == false)
				{
					await c.RejectMessage(string.Format(Realm.GetMessage("player_not_idle"), c.User.Mention, player.CurrentActionDisplay));
					return;
				}

				if (player.Skills?.Count == 0)
				{
					await c.RejectMessage($"{c.User.Mention} has no skills, poor you");
					return;
				}

				var skill = await session.Query<Skill>()
					.FirstOrDefaultAsync(s => s.DisplayName.Equals(skillName, StringComparison.OrdinalIgnoreCase));

				// TODO: Skill == null catch
				if (skill.IsActivatable == false)
				{
					await c.RejectMessage($"{c.User.Mention}, that skill cannot be activated");
					return;
				}

				var playerSkill = player.Skills?.FirstOrDefault(ps => ps.Id == skill.Id);
				if (playerSkill == null)
				{
					await c.RejectMessage($"{c.User.Mention} tries and tries, but nothing happens");
					return;
				}

				if (playerSkill.CooldownUntil > DateTime.Now)
				{
					var remaining = playerSkill.CooldownUntil - DateTime.Now;
					await c.ConfirmMessage($"{c.User.Mention}. That skill is still on cooldown ({(int)remaining.TotalSeconds}s more)");
					return;
				}

				var sType = Realm.GetSkillImplementation(skill.SkillImpl);
				if (sType == null)
				{
					await c.RejectMessage($"An error occured. Contact one of the admins (Error_SkillWitouthSkillImpl: {skill.Id})");
					return;
				}

				var sImpl = (Skills.ISkill)Activator.CreateInstance(sType);
				await sImpl.ExecuteSkill(c, skill, playerSkill, player, target);

				if (skill.CooldownRanks?.Count > 0 && sImpl.DoCooldown)
				{
					playerSkill.CooldownUntil = DateTime.Now.AddSeconds(playerSkill.Rank > skill.CooldownRanks.Count
						? skill.CooldownRanks[skill.CooldownRanks.Count - 1]
						: skill.CooldownRanks[playerSkill.Rank - 1]);
				}

				if (session.Advanced.HasChanges)
				{
					session.Advanced.IgnoreChangesFor(skill);
					session.Advanced.IgnoreChangesFor(playerSkill);

					await session.SaveChangesAsync();
				}
			}
		}
	}
}
