namespace RealmRpgBot.Buildings
{
	using System.Threading.Tasks;

	using DSharpPlus.CommandsNext;

	using Models;

    public interface IBuilding
    {
		Task EnterBuilding(CommandContext c, Building building);
    }
}
