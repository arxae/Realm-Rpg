namespace RealmRpgBot
{
	using System;
	using System.Linq;

	using Serilog;

	class RealmMain
	{
		static void Main(string[] args)
		{
			var logsStore = new Raven.Client.Documents.DocumentStore
			{
				Urls = Realm.GetDbServerUrls(),
				Database = "rpg_logs"
			}.Initialize();

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.Enrich.FromLogContext()
				.WriteTo.Console(
					outputTemplate: "{Timestamp:HH:mm:ss} [{SourceContext}] [{Level:u3}] - {Message}{NewLine}{Exception}")
				.WriteTo.RavenDB(logsStore,
					restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
				.CreateLogger();

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => Log.Logger.Error((Exception)e.ExceptionObject, "Unhandled Exception");

			var indexCount = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.IsClass && t.IsNested == false && t.Namespace == "RealmRpgBot.Index")
				.Count();

			Log.Logger.Information("Deploying {n} indexes", indexCount);
			Raven.Client.Documents.Indexes.IndexCreation.CreateIndexes(System.Reflection.Assembly.GetExecutingAssembly(), Db.DocStore);

			Realm.SetupDbSubscriptions();

			new Bot.RpgBot()
				.StartBotAsync()
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}
	}
}
