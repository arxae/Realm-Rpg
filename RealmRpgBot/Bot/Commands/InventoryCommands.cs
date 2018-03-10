﻿namespace RealmRpgBot.Bot.Commands
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;

	using Models.Character;
	using Models.Inventory;

	[Group("inv"), Description("Inventory Commands"), RequireRoles(RoleCheckMode.Any, new[] { Constants.ROLE_PLAYER })]
	public class InventoryCommands : RpgCommandBase
	{
		[Command("list"), Description("List your inventory")]
		public async Task ListInventory(CommandContext c)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Item>(i => i.Id)
					.LoadAsync<Player>(c.User.Id.ToString());
				if (player == null)
				{
					await c.RespondAsync(Realm.GetMessage("user_not_registered"));
					await c.RejectMessage();

					return;
				}

				var items = await session.LoadAsync<Item>(player.Inventory.Select(inv => inv.ItemId));

				var itemList = new System.Text.StringBuilder();
				foreach (var item in player.Inventory)
				{
					var itemDef = items[item.ItemId];
					itemList.AppendLine($"{itemDef.DisplayName} *x{item.Amount}*");
				}

				var embedBuilder = new DiscordEmbedBuilder()
					.WithTitle("Inventory")
					.WithDescription(itemList.ToString());

				await c.Member.SendMessageAsync(embed: embedBuilder.Build());
				await c.ConfirmMessage();
			}
		}
	}
}
