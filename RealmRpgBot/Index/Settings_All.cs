namespace RealmRpgBot.Index
{
	using System.Linq;

	using Raven.Client.Documents.Indexes;

	using Models;

	public class Settings_All:AbstractIndexCreationTask<Setting>
	{
		public Settings_All()
		{
			Map = settings => from s in settings
							 select new
							 {
								 s.Id
							 };
		}
	}
}
