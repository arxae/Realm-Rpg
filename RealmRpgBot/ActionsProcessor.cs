namespace RealmRpgBot
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using RealmRpgBot.Models;

	public class ActionsProcessor
	{
		readonly CommandContext cmdContext;
		public DiscordMessage AssociatedMessage { get; set; }

		public ActionsProcessor(CommandContext c, DiscordMessage msg)
		{
			cmdContext = c;
			AssociatedMessage = msg;
		}

		internal async Task ProcessActionList(List<string> actions)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				await ExecuteActionAsync(actions[i]);
			}
		}

		internal async Task ExecuteActionAsync(string action)
		{
			var l = Serilog.Log.ForContext<ActionsProcessor>();

			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session.LoadAsync<Player>(cmdContext.User.Id.ToString());
				if (player == null)
				{
					l.Warning("Trying to execute actions for player {pl}, but wasn't found", cmdContext.GetFullUserName());
					return;
				}

				try
				{
					// Set the associated msg to a specific value
					if (action.StartsWith("setmsg->", StringComparison.OrdinalIgnoreCase))
					{
						if (AssociatedMessage == null) return;
						string msg = ParseMessageMentions(cmdContext, $"{action.Substring("setmsg->".Length)}");
						await AssociatedMessage.ModifyAsync(msg, null);
						return;
					}

					// Heal the person who requested the command
					if (action.StartsWith("healply->", StringComparison.OrdinalIgnoreCase))
					{
						var parts = action.Split(new[] { "->" }, StringSplitOptions.None);
						if (int.TryParse(parts[1], out int healAmount) == false)
						{
							l.Error("Error processing action params: {cmd}", action);
							return;
						}

						HealPlayer(player, healAmount);
						return;
					}


					// Heal all health
					if (action.StartsWith("healplyfull->", StringComparison.OrdinalIgnoreCase))
					{
						FullHealPlayer(player);
						return;
					}

					// Respond with a message
					if (action.StartsWith("reply->", StringComparison.OrdinalIgnoreCase))
					{
						var msg = ParseMessageMentions(cmdContext, action.Substring("reply->".Length));
						await cmdContext.RespondAsync(msg);
						return;
					}

					// Delete the original message from the user
					if (action.StartsWith("deletemsg->", StringComparison.OrdinalIgnoreCase))
					{
						await cmdContext.Message.DeleteAsync();
						return;
					}

					// Delete the associated message (bot reply to user command)
					if (action.StartsWith("deleteassocmsg->", StringComparison.OrdinalIgnoreCase))
					{
						if (AssociatedMessage == null) return;
						await AssociatedMessage.DeleteAsync();
						return;
					}

					l.Warning("Unrecognized action: {cmd}", action);

					if (session.Advanced.HasChanged(player))
					{
						await session.SaveChangesAsync();
					}
				}
				catch
				{

					throw;
				}
			}
		}

		void HealPlayer(Player player, int amount)
		{
			player.HpCurrent += amount;
			if (player.HpCurrent > player.HpMax)
			{
				player.HpCurrent = player.HpMax;
			}
		}

		void FullHealPlayer(Player player)
		{
			player.HpCurrent = player.HpMax;
		}

		string ParseMessageMentions(CommandContext c, string msg)
		{
			return msg.Replace("@mention", c.User.Mention);
		}
	}
}
