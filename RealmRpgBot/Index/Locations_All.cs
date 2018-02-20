namespace RealmRpgBot.Index
{
	using System.Linq;

	using Raven.Client.Documents.Indexes;

	using Models;

	public class Locations_All : AbstractIndexCreationTask<Location>
	{
		public Locations_All()
		{
			Map = locations => from l in locations
							   select new
							   {
								   l.Id,
								   l.DisplayName
							   };
		}
	}
}
