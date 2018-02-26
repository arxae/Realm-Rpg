namespace RealmRpgBot
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;

	using DSharpPlus;
	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using Raven.Client.Documents.Session;

	public static class Extensions
	{
		// Logging
		static void Log(this CommandContext c, LogLevel level, string application, string message)
		{
			if (application == null)
			{
				application = "RealmBot";
			}

			c.Client.DebugLogger.LogMessage(level, application, message, System.DateTime.Now);
		}

		public static void LogCritical(this CommandContext c, string message, string application = null) => c.Log(LogLevel.Critical, application, message);
		public static void LogDebug(this CommandContext c, string message, string application = null) => c.Log(LogLevel.Debug, application, message);
		public static void LogInfo(this CommandContext c, string message, string application = null) => c.Log(LogLevel.Info, application, message);
		public static void LogWarning(this CommandContext c, string message, string application = null) => c.Log(LogLevel.Warning, application, message);

		// Discord
		public static string GetFullUsername(this DiscordUser user) => $"{user.Username}#{user.Discriminator}";
		public static string GetFullUserName(this CommandContext c) => c.User.GetFullUsername();
		public static async Task ConfirmMessage(this CommandContext c) => await c.Message.CreateReactionAsync(DiscordEmoji.FromName(c.Client, Constants.EMOJI_GREEN_CHECK));
		public static async Task RejectMessage(this CommandContext c) => await c.Message.CreateReactionAsync(DiscordEmoji.FromName(c.Client, Constants.EMOJI_RED_CROSS));

		// Database
		public static Models.ListObject GetList(this IDocumentSession session, string listname) => session.Load<Models.ListObject>(listname);
		public static async Task<Models.ListObject> GetList(this IAsyncDocumentSession session, string listname) => await session.LoadAsync<Models.ListObject>(listname);

		// Misc
		public static List<string> SplitCaseIgnore(this string input, string[] splitChars, bool trimSpaces = false)
		{
			var result = new List<string>();
			foreach (string split in splitChars)
			{
				if (splitChars.Count() == 1)
				{
					result.AddRange(Regex.Split(input, split, RegexOptions.IgnoreCase).ToList());
				}
				else
				{
					List<string> newString = Regex.Split(input, split, RegexOptions.IgnoreCase).ToList();
					foreach (string newStr in newString)
					{
						List<string> newSplt = splitChars.ToList();
						newSplt.Remove(split);
						return SplitCaseIgnore(newStr, newSplt.ToArray());
					}
				}
			}

			return trimSpaces == false
				? result
				: result.Select(s => s.Trim()).ToList();
		}

		public static string ReplaceCaseInsensitive(this string input, string oldStr, string newStr)
		{
			return Regex.Replace(input, Regex.Escape(oldStr), newStr.Replace("$", "$$"), RegexOptions.IgnoreCase);
		}

		public static T GetRandomEntry<T>(this List<T> list)
		{
			return list[DiceNotation.SingletonRandom.Instance.Next(list.Count - 1)];
		}

		public static T GetRandomEntry<T>(this T[] arr)
		{
			return arr[DiceNotation.SingletonRandom.Instance.Next(arr.Length)];
		}
	}
}
