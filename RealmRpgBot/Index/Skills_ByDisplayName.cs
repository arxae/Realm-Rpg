namespace RealmRpgBot.Index
{
	using System.Linq;

	using Raven.Client.Documents.Indexes;

	using Models;

	public class Skills_ByDisplayName : AbstractIndexCreationTask<Skill>
	{
		public Skills_ByDisplayName()
		{
			Map = skills => from s in skills
							  select new
							  {
								  s.DisplayName
							  };
		}
	}
}
