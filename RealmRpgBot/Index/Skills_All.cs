namespace RealmRpgBot.Index
{
	using System.Linq;

	using Raven.Client.Documents.Indexes;

	using Models;

	public class Skills_All : AbstractIndexCreationTask<Skill>
	{
		public Skills_All()
		{
			Map = skills => from s in skills
							  select new
							  {
								  s.Id,
								  s.DisplayName
							  };
		}
	}
}
