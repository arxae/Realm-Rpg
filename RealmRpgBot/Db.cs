namespace RealmRpgBot
{
	using System;

	using Raven.Client.Documents;

	using JSON = Newtonsoft.Json.JsonConvert;
	using Certificate = System.Security.Cryptography.X509Certificates.X509Certificate2;

	public class Db
	{
		private static readonly Lazy<IDocumentStore> storeInstance = new Lazy<IDocumentStore>(CreateStore);
		public static IDocumentStore DocStore => storeInstance.Value;

		static IDocumentStore CreateStore()
		{
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
	}
}
