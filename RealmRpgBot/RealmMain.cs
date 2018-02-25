namespace RealmRpgBot
{
	using System.Linq;
	using System.Reflection;

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
					outputTemplate: "{Timestamp:HH:mm:ss} [{SourceContext}] [{Level:u3}] - {Message}{NewLine}{Exception}",
					theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Literate)
				.WriteTo.RavenDB(logsStore,
					restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
				.CreateLogger();

			// Deploy/Update indexes
			var indexCount = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Count(t => t.IsClass && t.IsNested == false && t.Namespace == "RealmRpgBot.Index");

			Log.Logger.Information("Deploying {n} indexes", indexCount);
			Raven.Client.Documents.Indexes.IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), Db.DocStore);

			// Setup DB subscriptions
			Realm.SetupDbSubscriptions();

			// Setup correct RNG implementation
			DiceNotation.SingletonRandom.Instance = new DiceNotation.Random.GaussianRandom();

			new Bot.RpgBot()
				.StartBotAsync()
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}
	}
}
