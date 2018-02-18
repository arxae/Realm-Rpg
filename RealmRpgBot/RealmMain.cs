namespace RealmRpgBot
{
	using System;
	using Serilog;

	class RealmMain
	{
		static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.Enrich.FromLogContext()
				.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{SourceContext}] [{Level:u3}] - {Message}{NewLine}{Exception}")
				.CreateLogger();

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => Log.Logger.Error((Exception)e.ExceptionObject, "Unhandled Exception");

			new Bot.RpgBot()
				.StartBotAsync()
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}
	}
}
