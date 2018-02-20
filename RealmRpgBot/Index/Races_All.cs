namespace RealmRpgBot.Index
{
	using System.Linq;

	using Raven.Client.Documents.Indexes;

	using Models;

	public class Races_All : AbstractIndexCreationTask<Race>
	{
		public Races_All()
		{
			Map = races => from r in races
							   select new
							   {
								   r.Id,
								   r.DisplayName
							   };
		}
	}
}
