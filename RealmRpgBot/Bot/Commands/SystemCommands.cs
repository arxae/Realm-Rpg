namespace RealmRpgBot.Bot.Commands
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;
	using DSharpPlus.Interactivity;

	[Group("sys"),
		Description("System commands"),
		RequireRoles(RoleCheckMode.Any, new[] { "Realm Admin" })]
	public class SystemCommands : RpgCommandBase
	{
		[Command("ping"), Aliases(new[] { "p" }), Description("Check bot responsiveness, replies with latency")]
		public async Task Ping(CommandContext c)
		{
			await c.RespondAsync($"Pong! ({c.Client.Ping}ms)");
			await c.ConfirmMessage();
		}

		[Command("import"), Description("Import json string")]
		public async Task ImportJson(CommandContext c,
			[Description("The exact name of the imported type")]  string typeName,
			[Description("Json string, single object"), RemainingText()] string json)
		{
			if (typeName.StartsWith("RealmRpgBot.Models.") == false) typeName = "RealmRpgBot.Models." + typeName;

			var import = Db.ImportJson(typeName, json);

			if (import == true)
			{
				await c.ConfirmMessage();
				await c.RespondAsync("Object imported");
			}
			else
			{
				await c.RejectMessage();
				await c.RespondAsync("Import failed");
			}
		}

		[Command("importjson"), Description("Import json string with no type checking")]
		public async Task ImportJson(CommandContext c,
			[Description("Json string"), RemainingText()] string json)
		{
			var import = Db.ImportJson(json);

			if (import == true)
			{
				await c.ConfirmMessage();
				await c.RespondAsync("Object imported");
			}
			else
			{
				await c.RejectMessage();
				await c.RespondAsync("Import failed");
			}
		}

		[Command("importjsonbatch"), Description("Import multiple json strings")]
		public async Task ImportJsonBatch(CommandContext c, string typeName)
		{
			if (typeName.StartsWith("RealmRpgBot.Models.") == false) typeName = "RealmRpgBot.Models." + typeName;

			await c.RespondAsync("Json will be imported at the end. When done, enter #done#");

			var lst = new System.Collections.Generic.List<string>();

			var interact = c.Client.GetInteractivity();
			bool isDone = false;

			while (isDone == false)
			{
				var msg = await interact.WaitForMessageAsync(xm => xm.Author.Id == c.User.Id);
				var text = msg.Message.Content;

				if (text.ToLower() == "#done#")
				{
					isDone = true;
				}
				else
				{
					lst.Add(text);
				}
			}

			var failedLst = new System.Collections.Generic.List<string>();

			foreach (var item in lst)
			{
				var import = Db.ImportJson(typeName, item);
				
				if (import == false)
				{
					failedLst.Add(item);
				}
			}

			await c.RespondAsync($"{lst.Count - failedLst.Count}/{lst.Count} imported.");
			if (failedLst.Count > 0)
			{
				await c.RespondAsync("Following messages failed:");
				foreach (var m in failedLst)
				{
					await c.RespondAsync(m);
				}

				await c.ConfirmMessage();
			}
		}

		[Command("typelist"), Aliases("tl"), Description("Get a list of used types")]
		public async Task GetTaskList(CommandContext c)
		{
			var types = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.IsClass && t.IsNested == false && t.Namespace == "RealmRpgBot.Models")
				.Select(t => t.FullName.Replace("RealmRpgBot.Models.", ""));

			var sb = new System.Text.StringBuilder();
			foreach (var t in types)
				sb.AppendLine(t);

			var embed = new DiscordEmbedBuilder()
				.WithTitle("Typelist")
				.WithDescription(sb.ToString())
				.Build();

			await c.ConfirmMessage();
			await c.RespondAsync(embed: embed);
		}

		[Command("typeinfo"), Description("Get information about a given type")]
		public async Task GetTypeInfo(CommandContext c, [Description("The name of the type")] string typeName)
		{
			var type = Assembly.GetExecutingAssembly().GetTypes()
				.FirstOrDefault(t => t.FullName.Equals($"RealmRpgBot.Models.{typeName}", StringComparison.OrdinalIgnoreCase));

			if (type == null)
			{
				await c.RespondAsync($"Type {typeName} not found");
			}

			var sb = new System.Text.StringBuilder();

			foreach (var prop in type.GetProperties())
			{
				var name = prop.Name;
				var dataType = prop.PropertyType.Name;

				sb.AppendLine($"{name} ({dataType})");
			}

			var embed = new DiscordEmbedBuilder()
				.WithTitle($"Type information for {type.Name}")
				.WithDescription(sb.ToString())
				.Build();

			await c.RejectMessage();
			await c.RespondAsync(embed: embed);
		}
	}
}
