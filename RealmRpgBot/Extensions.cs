namespace RealmRpgBot
{
	using System;
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

		/// <summary>
		/// Extension method for generic IEnumerable/List collection allowing printing the contents.  Takes the characters to surround the list in,
		/// the method to use to get the string representation of each element (defaulting to the ToString function of type T),
		/// and the characters to use to separate the list elements.
		/// </summary>
		/// <remarks> Defaults to a representation looking something like [elem1, elem2, elem3].</remarks>
		/// <typeparam name="T">Type of elements in the IEnumerable.</typeparam>
		/// <param name="enumerable">IEnumerable to stringify -- never specified manually as this is an extension method.</param>
		/// <param name="begin">Character(s) that should precede the list elements.</param>
		/// <param name="elementStringifier">Function to use to get the string representation of each element. Null uses the ToString function of type T.</param>
		/// <param name="separator">Characters to separate the list by.</param>
		/// <param name="end">Character(s) that should follow the list elements.</param>
		/// <returns>A string representation of the IEnumerable.</returns>
		public static string ExtendToString<T>(this IEnumerable<T> enumerable, string begin = "[", Func<T, string> elementStringifier = null, string separator = ", ", string end = "]")
		{
			if (elementStringifier == null)
				elementStringifier = (T obj) => obj.ToString();

			string result = begin;
			bool first = true;
			foreach (var item in enumerable)
			{
				if (first)
					first = false;
				else
					result += separator;

				result += elementStringifier(item);
			}
			result += end;

			return result;
		}
	}
}
