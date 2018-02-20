namespace RealmRpgBot
{
	using System;

	using Serilog;

	class RealmMain
	{
		static void Main(string[] args)
		{
			//var logsStore = new Raven.Client.Documents.DocumentStore
			//{
			//	Urls = Realm.GetDbServerUrls(),
			//	Database = "rpg_logs"
			//}.Initialize();

			//Log.Logger = new LoggerConfiguration()
			//	.MinimumLevel.Verbose()
			//	.Enrich.FromLogContext()
			//	.WriteTo.Console(
			//		outputTemplate: "{Timestamp:HH:mm:ss} [{SourceContext}] [{Level:u3}] - {Message}{NewLine}{Exception}")
			//	.WriteTo.RavenDB(logsStore,
			//		restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
			//	.CreateLogger();

			//AppDomain.CurrentDomain.UnhandledException += (sender, e) => Log.Logger.Error((Exception)e.ExceptionObject, "Unhandled Exception");

			//new Bot.RpgBot()
			//	.StartBotAsync()
			//	.ConfigureAwait(false)
			//	.GetAwaiter()
			//	.GetResult();

			Console.WriteLine(Realm.GetNextXp(1));
			Console.WriteLine(Realm.GetNextXp(2));
			Console.WriteLine(Realm.GetNextXp(3));
			Console.WriteLine(Realm.GetNextXp(4));
			Console.WriteLine(Realm.GetNextXp(5));
			Console.ReadKey();
		}
	}
}
