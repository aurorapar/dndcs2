using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Menu;
using Dndcs2.constants;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.menus;

public class DndMainMenu : PlayerMenu
{
    public DndMainMenu() : base ("D&D Main Menu", Dndcs2.Instance)
    {
        ExitButton = true;
        AddItem("D&D Info", (player, option) => ShowInfo(player, option));
        AddItem("Select Class", (player, option) => ShowClasses(player, option));
        AddItem("Select Specie", (player, option) => ShowSpecies(player, option));
        AddItem("Player Stats", (player, option) => PlayerStats(player, option));
        AddItem("Player Info", (player, option) => PlayerInfo(player, option));
    }

    public void ShowInfo(CCSPlayerController player, ItemOption selectedOption)
    {
        Dndcs2.ShowDndInfo(player);
    }
    
    public void ShowClasses(CCSPlayerController player, ItemOption selectedOption)
    {
        PlayerMenu dndClasses = new("[D&D Classes]", Dndcs2.Instance);
        foreach (var dndClass in Enum.GetValues(typeof(constants.DndClass)).Cast<DndClass>())
        {
            dndClasses.AddItem(dndClass.ToString().Replace("_", " "), (player, option) => SelectClass(player, option));
        }
        dndClasses.PrevMenu = new DndMainMenu();
        dndClasses.Display(player, 10);

    }

    public void SelectClass(CCSPlayerController player, ItemOption selectedOption)
    {
        MessagePlayer(player, $"You picked {selectedOption.Text}");
    }
    
    public void ShowSpecies(CCSPlayerController player, ItemOption selectedOption)
    {
        PlayerMenu dndSpecies = new("[D&D Species]", Dndcs2.Instance);
        foreach (var dndSpecie in Enum.GetValues(typeof(constants.DndSpecie)).Cast<DndSpecie>())
        {
            dndSpecies.AddItem(dndSpecie.ToString().Replace("_", " "), (player, option) => SelectSpecie(player, option));
        }
        dndSpecies.PrevMenu = new DndMainMenu();
        dndSpecies.Display(player, 10);
    }

    public void SelectSpecie(CCSPlayerController player, ItemOption selectedOption)
    {
        MessagePlayer(player, $"You picked {selectedOption.Text}");
    }
    
    public void PlayerStats(CCSPlayerController player, ItemOption selectedOption)
    {
        MessagePlayer(player, "Use the command 'dndxp'");
    }
    
    public void PlayerInfo(CCSPlayerController player, ItemOption selectedOption)
    {
        MessagePlayer(player, "Use the command 'playerinfo [player name]'");
    }
}