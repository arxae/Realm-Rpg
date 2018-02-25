using Raven.Client.Documents.Session;

namespace RealmRpgBot.Skills
{
	using System.Linq;
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using DSharpPlus.Interactivity;
	using DiceNotation;

	using Models;

	public class Perception : ISkill
	{
		Serilog.ILogger log;
		CommandContext cmdCtx;
		IAsyncDocumentSession dbSession;

		public Perception()
		{
			log = Serilog.Log.ForContext<Perception>();
		}

		public async Task ExecuteSkill(CommandContext c, Skill skill, TrainedSkill trainedSkill, Player source, object target)
		{
			cmdCtx = c;

			dbSession = Db.DocStore.OpenAsyncSession();

			// Get locations
			var location = await dbSession
				.Include<Item>(i => i.Id)
				.LoadAsync<Location>(source.CurrentLocation);

			if (location.Perceptables == null || location.Perceptables.Count == 0)
			{
				await c.RespondAsync("Nothing to see here");
				await c.ConfirmMessage();
				return;
			}

			var roll = Dice.Roll($"1d20+{trainedSkill.Rank}");
			var foundPerceptables = location.Perceptables.Where(p => p.Difficulty <= roll).ToList();

			// Select a random one
			var perceptable = foundPerceptables.GetRandomEntry();

			if (perceptable.Type == Perceptable.PerceptableType.Item)
			{
				var item = await dbSession.LoadAsync<Item>(perceptable.DocId);


				var existingInventory = location.LocationInventory.FirstOrDefault(li => li.DocId == perceptable.DocId);
				if (existingInventory == null)
				{
					var inv = new LocationInventoryItem
					{
						DocId = item.Id,
						DisplayName = item.DisplayName,
						Amount = 1,
						DecaysOn = System.DateTime.Now.AddMinutes(15)
					};
					location.LocationInventory.Add(inv);
				}
				else
				{
					existingInventory.Amount += 1;
					// Add a couple of minutes to the location
					// TODO: Higher perception, adds more minutes (1m/30s for each rank)
					existingInventory.DecaysOn = existingInventory.DecaysOn.AddMinutes(1);
				}
			}
			//	else if (perceptable.Type == Perceptable.PerceptableType.Event) await ProcessPerceptableEvent(perceptable);

			if (dbSession.Advanced.HasChanges)
			{
				await dbSession.SaveChangesAsync();
			}

			dbSession.Dispose();

			await c.ConfirmMessage();
		}
	}
}
