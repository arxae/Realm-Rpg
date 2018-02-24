namespace RealmRpgBot.Skills
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using Raven.Client.Documents.Attachments;
	using Raven.Client.Documents.Operations.Attachments;

	using Models;

	public class Projectile : ISkill
	{
		public async Task ExecuteSkill(CommandContext c, Skill skill, Player source, object target)
		{
			if (target == null)
			{
				await c.RespondAsync($"{skill.DisplayName} requires a target");
				await c.RejectMessage();
				return;
			}

			if (Realm.GetTargetTypeFromId((string)target) != Enums.TargetType.User)
			{
				await c.RespondAsync("That is not a valid target");
				await c.RejectMessage();
				return;
			}

			var attachment = await Db.DocStore.Operations.SendAsync(new GetAttachmentOperation(skill.Id, "action.lua", AttachmentType.Document, null));
			string script = await new System.IO.StreamReader(attachment.Stream).ReadToEndAsync();
			await ScriptRunner.Get.PerformScriptAsync(c, script,
				c.User.Id.ToString(),
				null);
		}
	}
}
