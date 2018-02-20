namespace RealmRpgBot.Index
{
	using System.Linq;

	using Raven.Client.Documents.Indexes;

	using Models;

	public class BuildingActions_All : AbstractIndexCreationTask<BuildingAction>
	{
		public BuildingActions_All()
		{
			Map = acts => from act in acts
						  select new
						  {
							  act.Id
						  };
		}
	}
}
