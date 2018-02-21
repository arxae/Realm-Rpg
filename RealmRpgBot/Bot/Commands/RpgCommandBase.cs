namespace RealmRpgBot.Bot.Commands
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	public class RpgCommandBase : BaseCommandModule
	{
		public override Task BeforeExecutionAsync(CommandContext c)
		{
			var l = Serilog.Log.Logger;
			Serilog.Context.LogContext.PushProperty("SourceContext", $"CommandGroup: {GetType().Name}");

			l.Information("{usrname} -> {cmdName} (args: {args})",
				c.GetFullUserName(),
				c.Command.QualifiedName,
				c.RawArgumentString.Trim());

			Serilog.Context.LogContext.Reset();

			return base.BeforeExecutionAsync(c);
		}
	}
}
