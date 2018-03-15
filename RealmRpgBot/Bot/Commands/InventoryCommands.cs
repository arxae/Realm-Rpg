namespace RealmRpgBot.Bot.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;
	using Raven.Client.Documents;

	using Models.Character;
	using Models.Inventory;

	[Group("inv"), Description("Inventory Commands"), RequireRoles(RoleCheckMode.Any, Constants.ROLE_PLAYER)]
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
				var itemDict = new SortedDictionary<Item.ItemTypes, List<string>>();

				foreach (var item in player.Inventory)
				{
					var itemDef = items[item.ItemId];
					if (itemDict.ContainsKey(itemDef.Type) == false) itemDict.Add(itemDef.Type, new List<string>());
					itemDict[itemDef.Type].Add($"{itemDef.DisplayName} *x{item.Amount}*");
				}

				var desc = new System.Text.StringBuilder();
				foreach (var e in itemDict)
				{
					desc.AppendLine($"*{e.Key}*");
					e.Value.ForEach(v => desc.AppendLine(v));
					desc.AppendLine();
				}

				var embed = new DiscordEmbedBuilder()
					.WithTitle("Inventory")
					.WithDescription(desc.ToString())
					.WithTimestamp(System.DateTime.Now);

				await c.Member.SendMessageAsync(embed: embed.Build());
				await c.ConfirmMessage();
			}
		}

		[Command("discard"), Aliases("remove", "rm", "del"), Description("Discard an item")]
		public async Task DiscardItem(CommandContext c,
			[Description("The amount to discard. If set to 0, everything will be discarded")] int amount,
			[Description("The name of the item to discard"), RemainingText] string itemName)
		{
			if (amount < 0)
			{
				await c.RejectMessage($"{c.User.Mention}, Discard amount should be positive");
				return;
			}

			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Item>(itm => itm.DisplayName)
					.LoadAsync<Player>(c.User.Id.ToString());

				var itemDef = await session.Query<Item>()
					.FirstOrDefaultAsync(i => i.DisplayName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));

				if (itemDef == null)
				{
					await c.RejectMessage($"{c.User.Mention}, Incorrect item");
					return;
				}

				if (amount != 0)
				{
					var entry = player.Inventory.FirstOrDefault(pi => pi.ItemId == itemDef.Id);

					if (entry.Amount - amount <= 0) amount = 0;
					else entry.Amount -= amount;
				}

				if (amount == 0) player.Inventory.RemoveAll(pi => pi.ItemId == itemDef.Id);

				if (session.Advanced.HasChanges) await session.SaveChangesAsync();
			}

			await c.ConfirmMessage("Item(s) have been discarded");
		}

		// TODO: Overload (use itemname) that just uses 1
		[Command("use"), Description("Use an item")]
		public async Task UseItem(CommandContext c,
			[Description(""), RemainingText] string itemName)
		{
			await UseItem(c, 1, itemName);
		}

		[Command("use"), Description("Use an item")]
		public async Task UseItem(CommandContext c,
			[Description("")] int amount,
			[Description(""), RemainingText] string itemName)
		{
			if (amount < 1)
			{
				await c.RejectMessage($"{c.User.Mention}, Amount to use should be positive");
				return;
			}

			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var player = await session
					.Include<Item>(itm => itm.DisplayName)
					.LoadAsync<Player>(c.User.Id.ToString());

				var itemDef = await session.Query<Item>()
					.FirstOrDefaultAsync(i => i.DisplayName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));

				if (itemDef == null)
				{
					await c.RejectMessage($"{c.User.Mention}, Incorrect item");
					return;
				}

				session.Advanced.IgnoreChangesFor(itemDef);

				var invEntry = player.Inventory.FirstOrDefault(pi => pi.ItemId == itemDef.Id);
				if (invEntry.Amount < amount)
				{
					await c.RejectMessage($"{c.User.Mention}, you don't have that many of that item");
					return;
				}

				for (int i = 0; i < amount; i++)
				{
					itemDef.UseOnSelf(player);
				}

				invEntry.Amount -= amount;

				if (session.Advanced.HasChanges)
				{
					await session.SaveChangesAsync();
				}

				await c.ConfirmMessage($"{c.User.Mention}, {itemDef.UsedResponse}");
			}
		}
	}
}
