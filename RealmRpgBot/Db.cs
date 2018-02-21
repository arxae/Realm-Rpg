namespace RealmRpgBot
{
	using System;

	using Raven.Client.Documents;

	using JSON = Newtonsoft.Json.JsonConvert;
	using Certificate = System.Security.Cryptography.X509Certificates.X509Certificate2;

	public class Db
	{
		private static Lazy<IDocumentStore> store = new Lazy<IDocumentStore>(CreateStore);
		public static IDocumentStore DocStore => store.Value;

		static IDocumentStore CreateStore()
		{
			//Serilog.Log.ForContext<Db>().Information("Initializing DocStore instance");
			//IDocumentStore s = new DocumentStore()
			//{
			//	//Urls = new[] { "http://localhost:9090" },
			//	Urls = Realm.GetDbServerUrls(),
			//	Database = "rpg"
			//};
			////Certificate=new System.Security.Cryptography.X509Certificates.X509Certificate2("RealmBot.pfx")
			////}.Initialize();

			//// Check for certificate
			//var cert = Realm.GetCertificate();
			//if(cert != string.Empty)
			//{
			//	string pw = Environment.GetEnvironmentVariable("REALMBOT_CERT_KEY", EnvironmentVariableTarget.User) ?? string.Empty;
			//	s.Certificate = new Certificate(cert, pw);
			//}
			//s.Initialize();

			string cert = Realm.GetCertificate();
			string pw = null;
			if(cert != string.Empty)
			{
				pw = Environment.GetEnvironmentVariable("REALMBOT_CERT_KEY", EnvironmentVariableTarget.User) ?? string.Empty;
			}

			IDocumentStore store = new DocumentStore()
			{
				Urls = Realm.GetDbServerUrls(),
				Database = "rpg",
				Certificate = cert == string.Empty ? null : new Certificate(cert, pw)
			}.Initialize();










			//store.Conventions.FindCollectionName = type =>
			//{
			//	if (typeof(Models.Race).IsAssignableFrom(type)) return "Races";

			//	return Raven.Client.Documents.Conventions.DocumentConventions.DefaultGetCollectionName(type);
			//};

			return store;
		}

		public static bool ImportJson(string typeName, string json)
		{
			try
			{

				var t = Type.GetType(typeName, true, true);

				var obj = JSON.DeserializeObject(json, t);

				using (var session = DocStore.OpenSession())
				{
					session.Store(obj);
					session.SaveChanges();
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool ImportJson(string json)
		{
			try
			{
				var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

				using (var session = DocStore.OpenSession())
				{
					session.Store(Newtonsoft.Json.Linq.JObject.Parse(json));
					session.SaveChanges();
				}

				return true;
			}
			catch
			{
				Serilog.Log.Error("Error while importing json: {j}", json);
				return false;
			}
		}
	}
}
