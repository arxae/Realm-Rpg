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

	public static class Extensions
	{
		// Logging
		/// <summary>
		/// Logs a message to dsp logger
		/// </summary>
		/// <param name="c">The <see cref="CommandContext"/> of the issued command</param>
		/// <param name="level">Severity of the log entry</param>
		/// <param name="application">The application/context name</param>
		/// <param name="message">The actual message</param>
		static void Log(this CommandContext c, LogLevel level, string application, string message)
		{
			if (application == null)
			{
				application = "RealmBot";
			}

			c.Client.DebugLogger.LogMessage(level, application, message, DateTime.Now);
		}

		/// <summary>
		/// Shorthand to log a critical message
		/// </summary>
		public static void LogCritical(this CommandContext c, string message, string application = null) => c.Log(LogLevel.Critical, application, message);

		/// <summary>
		/// Shorthand to log a debug message
		/// </summary>
		public static void LogDebug(this CommandContext c, string message, string application = null) => c.Log(LogLevel.Debug, application, message);

		/// <summary>
		/// Shorthand to log a information message
		/// </summary>
		public static void LogInfo(this CommandContext c, string message, string application = null) => c.Log(LogLevel.Info, application, message);

		/// <summary>
		/// Shorthand to log a warning message
		/// </summary>
		public static void LogWarning(this CommandContext c, string message, string application = null) => c.Log(LogLevel.Warning, application, message);

		// Discord
		/// <summary>
		/// Gets the username#discriminator from a command issuer
		/// </summary>
		/// <param name="c"></param>
		/// <returns>username#discriminator</returns>
		public static string GetFullUserName(this CommandContext c) => $"{c.User.Username}#{c.User.Discriminator}";

		/// <summary>
		/// Assigns a message as confirmed (with optional message)
		/// </summary>
		/// <param name="c"></param>
		/// <param name="message">(Optional) Provide a message that will be replied before the command message is marked as confirmed.</param>
		/// <returns></returns>
		public static async Task ConfirmMessage(this CommandContext c, string message = null)
		{
			if (message != null) { await c.RespondAsync(message); }
			await c.Message.CreateReactionAsync(DiscordEmoji.FromName(c.Client, Realm.GetEmoji("confirm")));
		}

		/// <summary>
		/// Assigns a message as rejected (with optional message)
		/// </summary>
		/// <param name="c"></param>
		/// <param name="message">(Optional) Provide a message that will be replied before the command message is marked as rejected.</param>
		/// <returns></returns>
		public static async Task RejectMessage(this CommandContext c, string message = null)
		{
			if (message != null) { await c.RespondAsync(message); }
			await c.Message.CreateReactionAsync(DiscordEmoji.FromName(c.Client, Realm.GetEmoji("reject")));
		}

		// Misc
		/// <summary>
		/// Splits a string, but ignores the case when splitting on a word
		/// </summary>
		/// <param name="input">String to split</param>
		/// <param name="splitChars">String(s) to split on</param>
		/// <param name="trimSpaces">If true, splitted parts will automatically be splitted.</param>
		/// <returns></returns>
		public static List<string> SplitCaseIgnore(this string input, string[] splitChars, bool trimSpaces = false)
		{
			var result = new List<string>();
			foreach (string split in splitChars)
			{
				if (splitChars.Length == 1)
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

			return trimSpaces == false ? result : result.Select(s => s.Trim()).ToList();
		}

		/// <summary>
		/// Replace a string, but ignore the case
		/// </summary>
		/// <param name="input">String where something has to be replaced</param>
		/// <param name="oldStr">What to replace</param>
		/// <param name="newStr">New string</param>
		/// <returns></returns>
		public static string ReplaceCaseInsensitive(this string input, string oldStr, string newStr)
		{
			return Regex.Replace(input, Regex.Escape(oldStr), newStr.Replace("$", "$$"), RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Gets a random entry from a list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The source list</param>
		/// <returns>A single random entry from the source list</returns>
		public static T GetRandomEntry<T>(this List<T> list)
		{
			if (list.Count == 0) return default(T);

			return list.Count == 1
				? list[0]
				: list[Rng.Instance.Next(list.Count - 1)];
		}

		/// <summary>
		/// Gets a random entry from a array
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="arr">The source array</param>
		/// <returns>A single random entry from the source array</returns>
		public static T GetRandomEntry<T>(this T[] arr)
		{
			if (arr.Length == 0) return default(T);

			return arr.Length == 1
				   ? arr[0]
					: arr[Rng.Instance.Next(arr.Length)];
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
			{
				elementStringifier = obj => obj.ToString();
			}

			var result = new System.Text.StringBuilder(begin);
			bool first = true;
			foreach (var item in enumerable)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					result.Append(separator);
				}

				result.Append(elementStringifier(item));
			}

			result.Append(end);

			return result.ToString();
		}
	}
}
