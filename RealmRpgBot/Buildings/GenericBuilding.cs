namespace RealmRpgBot.Buildings
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using DSharpPlus.Interactivity;
	using Raven.Client.Documents.Attachments;
	using Raven.Client.Documents.Operations.Attachments;

	using Models.Map;

	public class GenericBuilding : IBuilding
	{
		public async Task EnterBuilding(CommandContext c, Building building)
		{
			List<BuildingAction> actions = new List<BuildingAction>();

			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var acts = await session.LoadAsync<BuildingAction>(building.Actions);
				actions.AddRange(acts.Values);
			}

			var desc = new System.Text.StringBuilder();

			if (building.WelcomeMessage != null)
			{
				desc.AppendLine($"*\"{building.WelcomeMessage}\"*");
				desc.AppendLine();
			}

			foreach (var act in actions)
			{
				desc.AppendLine(act.Description);
			}

			var embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.Blurple)
				.WithTitle(building.Name)
				.WithDescription(desc.ToString())
				.WithFooter(Realm.GetMessage("building_timeout"));
			var playerRespondMsg = await c.RespondAsync(embed: embed);

			foreach (var act in actions)
			{
				await playerRespondMsg.CreateReactionAsync(DiscordEmoji.FromName(c.Client, act.ReactionIcon));
			}

			await c.ConfirmMessage();

			var interact = c.Client.GetInteractivity();
			var response = await interact.WaitForMessageReactionAsync(playerRespondMsg, c.User, TimeSpan.FromSeconds(15));

			if (response == null)
			{
				await c.RejectMessage();
				await playerRespondMsg.DeleteAsync();

				return;
			}

			await playerRespondMsg.DeleteAllReactionsAsync();
			var responseName = response.Emoji.GetDiscordName().ToLower();

			var buildingActionId = actions.FirstOrDefault(ba => ba.ReactionIcon.Equals(responseName))?.Id;

			if (buildingActionId == null)
			{
				Serilog.Log.ForContext<GenericBuilding>().Error("Could not find BuildingAction with id {response}", responseName);
				await c.RespondAsync("An error occured. Contact one of the admins and (Error_BuildingActionReactionIdNotFound)");
				await c.RejectMessage();
				return;
			}

			var attachment = await Db.DocStore.Operations.SendAsync(new GetAttachmentOperation(buildingActionId, "action.lua", AttachmentType.Document, null));
			string script = await new System.IO.StreamReader(attachment.Stream).ReadToEndAsync();

			await playerRespondMsg.DeleteAsync();
			await new ScriptRunner(c, playerRespondMsg).PerformScriptAsync(script);
		}
	}
}
