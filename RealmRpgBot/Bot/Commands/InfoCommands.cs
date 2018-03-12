﻿namespace RealmRpgBot.Bot.Commands
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.CommandsNext.Attributes;
	using DSharpPlus.Entities;
	using Raven.Client.Documents;

	using Models.Character;

	[Group("info")]
	public class InfoCommands : RpgCommandBase
	{
		[Command("races"), Description("List available races")]
		public async Task ListRaces(CommandContext c)
		{
			using (var s = Db.DocStore.OpenAsyncSession())
			{
				var raceList = await s.Query<Race>().ToListAsync();

				var sb = new System.Text.StringBuilder();
				foreach (var r in raceList) sb.AppendLine($"- {r.DisplayName}");

				var embed = new DiscordEmbedBuilder()
					.WithTitle("Available races:")
					.WithDescription(sb.ToString())
					.WithFooter("TODO: .info race human")
					.Build();

				await c.RespondAsync(embed: embed);
			}

			await c.ConfirmMessage();
		}

		[Command("race"), Description("List available races")]
		public async Task ListRaces(CommandContext c,
			[Description("")] string raceName)
		{
			using (var s = Db.DocStore.OpenAsyncSession())
			{
				var race = await s.Query<Race>().FirstOrDefaultAsync(rn => rn.DisplayName == raceName);

				if (race == null)
				{
					await c.RejectMessage("This is not a valid race");
					return;
				}

				var raceInfo = new DiscordEmbedBuilder()
					.WithTitle($"Race Information - {race.DisplayName}");

				if (string.IsNullOrWhiteSpace(race.ImageUrl) == false)
				{
					raceInfo.WithImageUrl(race.ImageUrl);
				}

				var descr = new System.Text.StringBuilder(race.Description)
					.AppendLine()
					.AppendLine("**Stat Bonusses**")
					.AppendLine($"{nameof(race.BonusStats.Strength)}: +{race.BonusStats.Strength}")
					.AppendLine($"{nameof(race.BonusStats.Agility)}: +{race.BonusStats.Agility}")
					.AppendLine($"{nameof(race.BonusStats.Stamina)}: +{race.BonusStats.Stamina}")
					.AppendLine($"{nameof(race.BonusStats.Wisdom)}: +{race.BonusStats.Wisdom}")
					.AppendLine($"{nameof(race.BonusStats.Intelligence)}: +{race.BonusStats.Intelligence}");

				raceInfo.WithDescription(descr.ToString());

				await c.RespondAsync(embed: raceInfo.Build());

				await c.ConfirmMessage();
			}
		}

		[Command("skill"), Description("Get information about a skill")]
		public async Task GetSkillInfo(CommandContext c,
			[Description("The name of the skill you want info on"), RemainingText] string skillName)
		{
			using (var session = Db.DocStore.OpenAsyncSession())
			{
				var skill = await session.Query<Skill>("Skills/All")
						.FirstOrDefaultAsync(s => s.DisplayName.Equals(skillName));

				if (skill == null)
				{
					await c.RespondAsync($"Skill *{skillName}* couldn't be found");
					await c.RejectMessage();
					return;
				}

				var embed = new DiscordEmbedBuilder()
					.WithTitle(skill.DisplayName)
					.WithDescription(skill.Description);

				if (skill.ImageUrl != null)
				{
					embed.WithThumbnailUrl("https://i.redd.it/frqc24q7u96z.jpg");
				}

				await c.RespondAsync(c.User.Mention, embed: embed);
			}

			await c.ConfirmMessage();
		}
	}
}
