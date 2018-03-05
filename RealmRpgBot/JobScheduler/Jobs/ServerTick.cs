namespace RealmRpgBot.JobScheduler.Jobs
{
	using System;
	using System.Linq;

	using Raven.Client.Documents.Session;

	using Models.Character;
	using Models.Map;

	public class ServerTick : FluentScheduler.IJob
	{
		Serilog.ILogger _log;

		public void Execute()
		{
			_log = Serilog.Log.ForContext<ServerTick>();

			using (var session = Db.DocStore.OpenSession())
			{
				var players = session.Query<Player>()
					.Where(p => p.CurrentAction != Constants.ACTION_IDLE || p.CurrentAction == string.Empty || p.CurrentAction == null);

				foreach (var player in players)
				{
					if (player.BusyUntil > DateTime.Now) continue;

					if (player.CurrentAction.StartsWith(Constants.ACTION_GATHERING, StringComparison.OrdinalIgnoreCase))
					{
						Gathering(player, session);
					}
					else if (player.CurrentAction.StartsWith(Constants.ACTION_REST, StringComparison.OrdinalIgnoreCase))
					{
						Resting(player);
					}
				}
			}
		}

		/// <summary>
		/// Player is gathering resources on a location
		/// </summary>
		/// <param name="p"></param>
		/// <param name="dbSession"></param>
		void Gathering(Player p, IDocumentSession dbSession)
		{
			var skillName = p.CurrentAction.Split(':')[1];
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

		/// <summary>
		/// Player is resting, healing 10% hp/tick
		/// </summary>
		/// <param name="p"></param>
		void Resting(Player p)
		{
			int heal = (p.HpMax / 100) * 10;// TODO: Add to settings
			p.HealHpAsync(heal).ConfigureAwait(false);
		}
	}
}
