namespace RealmRpgBot.Bot.Commands
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	public class RpgCommandBase : BaseCommandModule
	{
		public override Task BeforeExecutionAsync(CommandContext c)
		{
			var l = Serilog.Log.Logger;
			l.ForContext(GetType());
			l.Information("{usrname} -> {cmdName} (args: {args})",
				c.GetFullUserName(),
				c.Command.QualifiedName,
				c.RawArgumentString.Trim());

			return base.BeforeExecutionAsync(c);
		}
	}
}
