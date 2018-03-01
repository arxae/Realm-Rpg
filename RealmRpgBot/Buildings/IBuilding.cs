namespace RealmRpgBot.Buildings
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	using Models.Map;

	public interface IBuilding
	{
		Task EnterBuilding(CommandContext c, Building building);
	}
}
