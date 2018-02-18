﻿namespace RealmRpgBot.Bot
{
	using System;
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
				Token = Environment.GetEnvironmentVariable("SONGBOT_KEY", EnvironmentVariableTarget.User),
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
			l.Information("Registered {n} commandgroup(s)", cmd.RegisteredCommands.Count - 1);

			// Interactivity
			Client.UseInteractivity(new InteractivityConfiguration());

			l.Information("Bot initialized");
		}

		private void DebugLogger_LogMessageReceived(object sender, DSharpPlus.EventArgs.DebugLogMessageEventArgs e)
		{
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
