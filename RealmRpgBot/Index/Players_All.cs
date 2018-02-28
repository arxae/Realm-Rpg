namespace RealmRpgBot.Index
{
	using System.Linq;

	using Raven.Client.Documents.Indexes;

	using Models;

	public class Players_All:AbstractIndexCreationTask<Player>
	{
		public Players_All()
		{
			Map = players => from p in players
							 select new
							 {
								 p.Id,
								 p.GuildId
							 };
		}
	}
}
