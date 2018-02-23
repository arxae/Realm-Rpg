﻿namespace RealmRpgBot.Buildings
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using DSharpPlus.Interactivity;

	using Models;
	using Raven.Client.Documents.Attachments;
	using Raven.Client.Documents.Operations.Attachments;

	public class Inn : IBuilding
	{
		public async Task EnterBuilding(CommandContext c, Building building)
		{
			List<BuildingAction> actions = new List<BuildingAction>();

			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var _acts = await session.LoadAsync<BuildingAction>(building.Actions);
				actions.AddRange(_acts.Values);
			}

			var desc = new System.Text.StringBuilder();
			foreach (var act in actions)
			{
				desc.AppendLine(act.Description);
			}

			var embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.Blurple)
				.WithTitle(building.Name)
				.WithDescription(desc.ToString())
				.WithFooter(Constants.MSG_BUILDING_TIMEOUT);
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

			var buildingActionId = actions.FirstOrDefault(ba => ba.ReactionIcon.Equals(responseName)).Id;

			var attachment = await Db.DocStore.Operations.SendAsync(new GetAttachmentOperation(buildingActionId, "action.lua", AttachmentType.Document, null));
			string script = await new System.IO.StreamReader(attachment.Stream).ReadToEndAsync();

			await playerRespondMsg.DeleteAsync();

			await ScriptRunner.Get.PerformScriptAsync(c, script,
				c.User.Id.ToString(),
				playerRespondMsg);
		}
	}
}
