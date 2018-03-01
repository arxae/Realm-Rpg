namespace RealmRpgBot.Skills
{
	using System;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	using Models.Character;
	using Models.Map;

	public class GatherSkill : ISkill
	{
		public bool DoCooldown { get; set; }

		public async Task ExecuteSkill(CommandContext c, Skill skill, TrainedSkill trainedSkill, Player source, object target)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var location = await session
					.Include<Skill>(i => i.Id)
					.LoadAsync<Location>(source.CurrentLocation);

				if (location.Resources?.Count == 0)
				{
					await c.RespondAsync("No resources available for this skill");
					await c.ConfirmMessage();
					return;
				}

				string resourceType;
				if (skill.Parameters.ContainsKey("ResourceType") == false)
				{
					await c.RespondAsync("An error occured. Contact one of the admins and (Error_NoResourceTypeForGatherSkill)");
					await c.RejectMessage();
					return;
				}
				else
				{
					resourceType = skill.Parameters["ResourceType"];
				}

				// Check if there is a resource type like this on the location
				if (location.Resources.Contains(resourceType))
				{
					source.BusyUntil = DateTime.Now.AddSeconds(skill.CooldownRanks[0]);
					source.CurrentAction = $"gathering:{skill.Id}";
					source.CurrentActionDisplay = skill.Parameters["ActionDisplayText"];

					DoCooldown = true;
				}

				if (session.Advanced.HasChanges)
				{
					await session.SaveChangesAsync();
				}
			}

			await c.ConfirmMessage();
		}
	}
}
