namespace RealmRpgBot.Skills
{
	using System.Linq;
	using System.Threading.Tasks;

	using DiceNotation;
	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using DSharpPlus.Interactivity;
	using Raven.Client.Documents.Session;

	using Models;
	using System;

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

				// TODO: Move to somewhere sensible/db
				var uncoverVariations = new[]
				{
					"under some bushes",
					"on the floor",
					"in some boxes",
					"under a bench",
					"behind a frog"
				};

				var existingInventory = location.LocationInventory.FirstOrDefault(li => li.DocId == perceptable.DocId);
				if (existingInventory == null)
				{
					var inv = new LocationInventoryItem
					{
						DocId = item.Id,
						DisplayName = item.DisplayName,
						Amount = perceptable.Count == 0
							? 1
							: perceptable.Count,
						DecaysOn = DateTime.Now.AddMilliseconds((trainedSkill.Rank * 2.5) * 10000)
					};
					location.LocationInventory.Add(inv);

					await c.RespondAsync($"{c.User.Mention} uncovered {item.DisplayName} {uncoverVariations.GetRandomEntry()} (Roll {roll})");
				}
				else
				{
					if (existingInventory.Amount < perceptable.MaxPerceptable)
					{
						existingInventory.Amount += perceptable.Count;
						existingInventory.DecaysOn = existingInventory.DecaysOn + TimeSpan.FromMilliseconds((trainedSkill.Rank * 2.5) * 10000);

						await c.RespondAsync($"{c.User.Mention} found some more {item.DisplayName} {uncoverVariations.GetRandomEntry()} (Roll {roll})");
					}
					else
					{
						await c.RespondAsync($"{c.User.Mention} looked for hours, but couldn't find anything");
					}
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
