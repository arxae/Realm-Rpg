using DSharpPlus.CommandsNext.Attributes;

namespace RealmRpgBot.Bot
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus;
	using DSharpPlus.CommandsNext;
	using DSharpPlus.Interactivity;

	public class RpgBot
	{
		public static DiscordClient Client;

		public RpgBot()
		{
			var l = Serilog.Log.ForContext<RpgBot>();

			Client = new DiscordClient(new DiscordConfiguration()
			{
				Token = Environment.GetEnvironmentVariable("REALMBOT_KEY", EnvironmentVariableTarget.User),
				TokenType = TokenType.Bot,
				LogLevel = LogLevel.Debug
			});

			Client.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;

			// Commands
			var cmd = Client.UseCommandsNext(new CommandsNextConfiguration
			{
				StringPrefixes = new[] { "." },
				CaseSensitive = false
			});

			cmd.RegisterCommands(System.Reflection.Assembly.GetExecutingAssembly());
			l.Information("Registered {n} command(s)", cmd.RegisteredCommands.Count - 1);

			cmd.CommandErrored += (e) =>
			{
				return Task.Run(async () =>
				{
					Serilog.Log.ForContext<RpgBot>().Error(e.Exception, "Exception happened");

					var checks = ((DSharpPlus.CommandsNext.Exceptions.ChecksFailedException)e.Exception).FailedChecks;
					// Check if the error is due to missing role
					if (checks.Any(x => x is RequireRolesAttribute))
					{
						using (var s = Db.DocStore.OpenAsyncSession())
						{
							var p = await s.LoadAsync<Models.Character.Player>(e.Context.User.Id.ToString());
							if (p == null)
							{
								await e.Context.Member.SendMessageAsync(Realm.GetMessage("not_registered"));
							}
							else
							{
								var missingRoles = ((RequireRolesAttribute)checks
										.FirstOrDefault(x => x is RequireRolesAttribute))?.RoleNames
									.ExtendToString("", null, ", ", "");

								await e.Context.Member.SendMessageAsync(string.Format(Realm.GetMessage("missing_roles"), missingRoles));
							}
						}

						e.Handled = true;
						return;
					}

					if (checks.Any(x => x is CooldownAttribute))
					{
						await e.Context.RespondAsync($"{e.Context.User.Mention}, you need to wait {((CooldownAttribute)checks[0]).Reset.TotalSeconds} more seconds.");
						e.Handled = true;
						return;
					}

					Serilog.Log.ForContext<RpgBot>()
						.Error(e.Exception, "Unhandled exception when executing command: {cmd}", e.Command.QualifiedName);
				});
			};

			// Interactivity
			Client.UseInteractivity(new InteractivityConfiguration());

			l.Information("Bot initialized");
		}

		private void DebugLogger_LogMessageReceived(object sender, DSharpPlus.EventArgs.DebugLogMessageEventArgs e)
		{
			if (e.Message == "Received WebSocket Heartbeat Ack" || e.Message == "Sending Heartbeat") return;

			var l = Serilog.Log.ForContext("SourceContext", e.Application);

			switch (e.Level)
			{
				case LogLevel.Debug: l.Debug(e.Message); break;
				case LogLevel.Info: l.Information(e.Message); break;
				case LogLevel.Warning: l.Warning(e.Message); break;
				case LogLevel.Error: l.Error(e.Message); break;
				case LogLevel.Critical: l.Fatal(e.Message); break;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		public async Task StartBotAsync()
		{
			await Client.ConnectAsync();
			await Task.Delay(-1);
		}
	}
}
