namespace RealmRpgBot.Buildings
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using DSharpPlus.Interactivity;

	using Models;

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
			var msg = await c.RespondAsync(embed: embed);

			foreach (var act in actions)
			{
				await msg.CreateReactionAsync(DiscordEmoji.FromName(c.Client, act.ReactionIcon));
			}

			await c.ConfirmMessage();

			var interact = c.Client.GetInteractivity();
			var response = await interact.WaitForMessageReactionAsync(msg, c.User, TimeSpan.FromSeconds(15));

			if (response == null)
			{
				await c.RejectMessage();
				await msg.DeleteAsync();

				return;
			}

			await msg.DeleteAllReactionsAsync();
			var responseName = response.Emoji.GetDiscordName().ToLower();

			var actionsToPerform = new List<string>(actions.FirstOrDefault(acts => acts.ReactionIcon.Equals(responseName)).ActionCommands);
			await msg.DeleteAsync();
			await new ActionsProcessor(c, msg :msg).ProcessActionList(actionsToPerform);
		}
	}
}
