namespace RealmRpgBot.JobScheduler.Jobs
{
	using System;

	using Models.Character;
	using Models.Map;

	/// <summary>
	/// Checks if a player will be attacked on a location
	/// </summary>
	/// TODO: Check when last combat happend (always let a minute pass)
	/// TODO: Force combat encounter
	public class LocationAttackTick : FluentScheduler.IJob
	{
		Serilog.ILogger _log;

		public void Execute()
		{
			_log = Serilog.Log.ForContext<LocationAttackTick>();

			using (var session = Db.DocStore.OpenSession())
			{
				var players = session.Query<Player>();

				using (var enumerator = session.Advanced.Stream(players))
				{
					while (enumerator.MoveNext())
					{
						var player = enumerator.Current?.Document;
						if (player == null) continue;
						var location = session.Load<Location>(player.CurrentLocation);

						var roll = Rng.Instance.Next(0, 100);
						if (location.SafetyRating.Equals("sanctuary", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}

						if (location.SafetyRating.Equals("friendly", StringComparison.OrdinalIgnoreCase))
						{
							if (roll == 1)
							{
								_log.Information("Force encounter for {pl}", player.Name);
							}
							continue;
						}

						if (location.SafetyRating.Equals("neutral", StringComparison.OrdinalIgnoreCase))
						{
							if (roll < 5)
							{
								_log.Information("Force encounter for {pl}", player.Name);
							}
							continue;
						}

						if (location.SafetyRating.Equals("caution", StringComparison.OrdinalIgnoreCase))
						{
							if (roll < 15)
							{
								_log.Information("Force encounter for {pl}", player.Name);
							}
							continue;
						}

						if (location.SafetyRating.Equals("dangerous", StringComparison.OrdinalIgnoreCase))
						{
							if (roll < 50)
							{
								_log.Information("Force encounter for {pl}", player.Name);
							}
							continue;
						}

						if (location.SafetyRating.Equals("hostile", StringComparison.OrdinalIgnoreCase))
						{
							_log.Information("Force encounter for {pl}", player.Name);
						}
					}
				}
			}
		}
	}
}
