namespace RealmRpgBot
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	using Raven.Client.Documents;

	using JSON = Newtonsoft.Json.JsonConvert;

	public class Db
	{
		private static Lazy<IDocumentStore> store = new Lazy<IDocumentStore>(CreateStore);
		public static IDocumentStore DocStore => store.Value;

		static IDocumentStore CreateStore()
		{
			IDocumentStore s = new DocumentStore()
			{
				//Urls = new[] { "http://localhost:9090" },
				Urls = Realm.GetDbServerUrls(),
				Database = "rpg"
			}.Initialize();

			//store.Conventions.FindCollectionName = type =>
			//{
			//	if (typeof(Models.Race).IsAssignableFrom(type)) return "Races";

			//	return Raven.Client.Documents.Conventions.DocumentConventions.DefaultGetCollectionName(type);
			//};

			return s;
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
