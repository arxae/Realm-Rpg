namespace RealmRpgBot.JobScheduler.Jobs
{
	using System;
	using System.Linq;

	using Raven.Client.Documents.Session;

	using Models.Character;
	using Models.Map;

	public class PlayerActionDurationCheck : FluentScheduler.IJob
	{
		Serilog.ILogger _log;

		public void Execute()
		{
			_log = Serilog.Log.ForContext<LocationInventoryDecayJob>();

			using (var session = Db.DocStore.OpenSession())
			{
				var players = session.Query<Player>()
					.Where(p => p.CurrentAction != "Idle" || p.CurrentAction == string.Empty || p.CurrentAction == null)
					.ToList();

				foreach (var player in players)
				{
					if (player.BusyUntil < DateTime.Now)
					{
						if (player.CurrentAction.StartsWith("Gathering:", StringComparison.OrdinalIgnoreCase))
						{
							Gathering(player, player.CurrentAction, session);
						}
					}
				}

				if (session.Advanced.HasChanges)
				{
					session.SaveChanges();
				}
			}
		}

		void Gathering(Player p, string actionString, IDocumentSession dbSession)
		{
			var skillName = actionString.Split(':')[1];
			var skill = dbSession
				.Include<Resource>(r => r.Id)
				.Load<Skill>(skillName);
			var resource = dbSession.Load<Resource>(skill.Parameters["ResourceType"]);

			var amt = Rng.Instance.Next(resource.HarvestQuantityMin, resource.HarvestQuantityMax);

			if (amt <= 0) return;

			_log.Debug("{player} received {item} x{amt}", p.Name, resource.HarvestedItemId, amt);
			p.AddItemToInventory(resource.HarvestedItemId, amt);
			p.SetIdleAction();

			// Warn player
			var discordMember = Bot.RpgBot.Client.GetGuildAsync(p.GuildId)
				.GetAwaiter().GetResult()
				.GetMemberAsync(ulong.Parse(p.Id))
				.GetAwaiter().GetResult();
			discordMember.SendMessageAsync($"Your {skill.DisplayName} action has completed and you gained {amt} pieces of {resource.DisplayName}")
				.GetAwaiter()
				.GetResult();
		}
	}
}
