namespace RealmRpgBot.Script
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;
	using DSharpPlus.Entities;
	using Microsoft.ClearScript;
	using Microsoft.ClearScript.V8;

	using Models.Character;

	// TODO: V8ScriptEngine pool
	public class ScriptManager
	{
		static Serilog.ILogger Log = Serilog.Log.ForContext<ScriptManager>();

		/// <summary>
		/// Executes a script in context of a discord reply
		/// </summary>
		/// <param name="scriptName">The name of the action that executes this script.</param>
		/// <param name="script">The script source code</param>
		/// <param name="c">CommandContext of the issued discord command</param>
		/// <param name="player">Player object of the user that issues the command</param>
		/// <param name="m">The message that the bot sent as a reply</param>
		/// <returns></returns>
		public static async Task RunDiscordScriptAsync(string scriptName, string script, CommandContext c, Player player, DiscordMessage m)
		{
			await Task.Run(() =>
			{
				using (var engine = new V8ScriptEngine())
				{
					if (engine.Compile(script, V8CacheKind.Parser, out byte[] cacheBytes) == null)
					{
						c.RespondAsync($"A exception occured while compiling the {scriptName} action script.");
						return;
					}

					using (var compiled = engine.Compile(script, V8CacheKind.Parser, cacheBytes, out bool cacheAccepted))
					{
						if (cacheAccepted)
						{
							Log.Debug("Using script cache for {scriptname}", scriptName);
						}

						engine.AddHostObject("Realm", new ScriptContext(player, c, m));
						engine.AddHostType(typeof(DiscordMessage));
						engine.AddHostType(typeof(CommandContext));
						engine.AddHostType(typeof(DiscordEmoji));
						engine.AddHostObject("Discord", new HostTypeCollection("DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity"));

						try
						{
							engine.Execute(compiled);
						}
						catch (ScriptEngineException e)
						{
							Log.Error(e, "Scripting Exception");
							c.RespondAsync($"A exception occured while running the {scriptName} action script: {e.Message}");
						}
					}
				}
			});
		}
	}
}
