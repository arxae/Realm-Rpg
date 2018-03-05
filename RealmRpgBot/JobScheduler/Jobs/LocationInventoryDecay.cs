namespace RealmRpgBot.JobScheduler.Jobs
{
	using System;
	using System.Linq;

	using Models.Map;

	public class LocationInventoryDecay : FluentScheduler.IJob
	{
		Serilog.ILogger _log;

		public void Execute()
		{
			_log = Serilog.Log.ForContext<LocationInventoryDecay>();

			using (var session = Db.DocStore.OpenSession())
			{
				var locs = session.Query<Location>()
					.Where(l => l.LocationInventory.Count > 0)
					.ToList();

				foreach (var loc in locs)
				{
					int removed = loc.LocationInventory.RemoveAll(i => i.DecaysOn < DateTime.Now);
					if (removed > 0)
					{
						_log.Debug("Removed {nr} decayed inventory from {locname}", removed, loc.Id);
					}
				}

				if (session.Advanced.HasChanges)
				{
					session.SaveChanges();
				}
			}
		}
	}
}
