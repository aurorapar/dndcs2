using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Menu;
using Dndcs2.constants;
using Dndcs2.Sql;
using Dndcs2.stats;
using static Dndcs2.constants.DndClassDescription;
using static Dndcs2.constants.DndSubClassDescription;
using static Dndcs2.constants.DndSpecieDescription;

namespace Dndcs2.menus;

public class DndMainMenu : PlayerMenu
{
    public DndMainMenu(CCSPlayerController player) : base ("D&D Main Menu", Dndcs2.Instance)
    {
        ExitButton = true;
        AddItem("D&D Info", (player, option) => ShowInfo(player, option));
        AddItem("Select Class", (player, option) => ShowClasses(player, option));
        if(CommonMethods.RetrievePlayerClassLevel(player) >= 3)
            AddItem("Select Sub Class", (player, option) => ShowSubClasses(player, option));
        AddItem("Select Specie", (player, option) => ShowSpecies(player, option));
        AddItem("Player Stats", (player, option) => ShowPlayerStats(player, option));
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
            dndClasses.AddItem(Dndcs2.FormatConstantNameForDisplay(dndClass.ToString()), (player, option) => SelectClass(player, option));
        }
        dndClasses.PrevMenu = new DndMainMenu(player);
        dndClasses.Display(player, 10);

    }    

    public void SelectClass(CCSPlayerController player, ItemOption selectedOption)
    {
        MenuManager.CloseActiveMenu(player);
        var selectedText = Dndcs2.FormatConstantNameForDisplay(selectedOption.Text);
        var dndClass = Enum.GetValues(typeof(DndClass)).Cast<DndClass>()
            .ToList().FirstOrDefault(c => Dndcs2.FormatConstantNameForDisplay(c.ToString()).Equals(selectedText, StringComparison.OrdinalIgnoreCase));
        var classDescription = DndClassDescriptions[dndClass];
        classDescription = "<font color=\"lime\">" + selectedOption.Text + "</font><br>" + classDescription;

        var dndPlayer = CommonMethods.RetrievePlayer(player);
        if(dndPlayer.DndClassId == (int)dndClass)
            classDescription += $"<br><font color=\"red\">You are already a {selectedOption.Text}</red>";
        else if(!CommonMethods.CanPlayClass(player, dndClass))        
            classDescription += $"<br><font color=\"red\">You do not meet the requirements to play {selectedOption.Text}</font>";
        else
        {
            classDescription += $"<br><font color=\"lime\">You will be a {selectedOption.Text} the next time you spawn.</font>";
            PlayerStats.GetPlayerStats(player).QueuedClass = dndClass;            
        }
        
        Server.NextFrame(() =>
        {
            player.PrintToCenterHtml(classDescription); 
        });
    }
    
    public void ShowSpecies(CCSPlayerController player, ItemOption selectedOption)
    {
        PlayerMenu dndSpecies = new("[D&D Species]", Dndcs2.Instance);
        foreach (var dndSpecie in Enum.GetValues(typeof(constants.DndSpecie)).Cast<DndSpecie>())
        
            dndSpecies.AddItem(Dndcs2.FormatConstantNameForDisplay(dndSpecie.ToString()), (player, option) => SelectSpecie(player, option));
        
        dndSpecies.PrevMenu = new DndMainMenu(player);
        dndSpecies.Display(player, 10);
    }

    public void SelectSpecie(CCSPlayerController player, ItemOption selectedOption)
    {
        MenuManager.CloseActiveMenu(player);
        var selectedText = Dndcs2.FormatConstantNameForDisplay(selectedOption.Text);
        var dndSpecie = Enum.GetValues(typeof(DndSpecie)).Cast<DndSpecie>()
            .ToList().FirstOrDefault(c => Dndcs2.FormatConstantNameForDisplay(c.ToString()).Equals(selectedText, StringComparison.OrdinalIgnoreCase));
        var specieDescription = DndSpecieDescriptions[dndSpecie];
        specieDescription = "<font color=\"lime\">" + selectedOption.Text + "</font><br>" + specieDescription;

        if(CommonMethods.RetrievePlayer(player).DndSpecieId == (int) dndSpecie)
            specieDescription += $"<br><font color=\"red\">You are already a {selectedOption.Text}</red>";
        else if(!CommonMethods.CanPlaySpecie(player, dndSpecie))
            specieDescription += $"<br><font color=\"red\">You do not meet the requirements to play {selectedOption.Text}</font>";
        else
        {
            specieDescription += $"<br><font color=\"lime\">You will be a {selectedOption.Text} the next time you spawn.</font>";
            PlayerStats.GetPlayerStats(player).QueuedSpecie = dndSpecie;
        }
        
        Server.NextFrame(() =>
        {
            player.PrintToCenterHtml(specieDescription); 
        });
    }
    
    public void ShowSubClasses(CCSPlayerController player, ItemOption selectedOption)
    {
        PlayerMenu dndSubClasses = new("[D&D Sub Classes]", Dndcs2.Instance);
        int playerClassId = 0;
        var playerStats = PlayerStats.GetPlayerStats(player);
        if(playerStats.QueuedClass != null)
            playerClassId = (int) playerStats.QueuedClass;
        else
            playerClassId = CommonMethods.RetrievePlayer(player).DndClassId;
        var playerClassLevel = CommonMethods.RetrievePlayerClassLevel(player);
        var relevantSubClassIds = Dndcs2.Instance.DndSubClassLookup
            .Where(kvp => kvp.Value.DndParentClassId == playerClassId)
            .Select(kvp => kvp.Value)
            .ToList();
        var validSubClasses = relevantSubClassIds
            .Where(sc => sc.DndParentClassLevelRequirementId <= playerClassLevel)
            .ToList();
        foreach (var dndSubClass in validSubClasses)
            dndSubClasses.AddItem(dndSubClass.DndSubClassName, (player, option) => SelectSubClass(player, option));
        dndSubClasses.PrevMenu = new DndMainMenu(player);
        dndSubClasses.Display(player, 10);
    }
    
    public void SelectSubClass(CCSPlayerController player, ItemOption selectedOption)
    {
        MenuManager.CloseActiveMenu(player);
        var selectedText = Dndcs2.FormatConstantNameForDisplay(selectedOption.Text);
        var dndSubClass = Enum.GetValues(typeof(DndSubClass)).Cast<DndSubClass>()
            .ToList().FirstOrDefault(c => Dndcs2.FormatConstantNameForDisplay(c.ToString()).Equals(selectedText, StringComparison.OrdinalIgnoreCase));
        var classDescription = DndSubClassDescriptions[dndSubClass];
        classDescription = "<font color=\"lime\">" + selectedOption.Text + "</font><br>" + classDescription;

        if (CommonMethods.RetrievePlayer(player).DndSubClassId == (int)dndSubClass)
            classDescription += $"<br><font color=\"red\">You are already a {selectedOption.Text}</red>";
        else if(!CommonMethods.CanPlayClass(player, dndSubClass))        
            classDescription += $"<br><font color=\"red\">You do not meet the requirements to play {selectedOption.Text}</font>";
        else
        {
            classDescription += $"<br><font color=\"lime\">You will be a {selectedOption.Text} the next time you spawn.</font>";
            PlayerStats.GetPlayerStats(player).QueuedSubClass = dndSubClass;
        }
        
        Server.NextFrame(() =>
        {
            player.PrintToCenterHtml(classDescription); 
        });
    }
    
    public void ShowPlayerStats(CCSPlayerController player, ItemOption selectedOption)
    {
        Dndcs2.ShowDndXp(player, player);
    }
    
    public void PlayerInfo(CCSPlayerController player, ItemOption selectedOption)
    {
        PlayerMenu playerInfoMenu = new("[D&D Player Info]", Dndcs2.Instance);
        foreach (var person in Utilities.GetPlayers())
            playerInfoMenu.AddItem(person.PlayerName, (player, option) => DoPlayerInfo(player, option));
        
        playerInfoMenu.PrevMenu = new DndMainMenu(player);
        playerInfoMenu.Display(player, 30);
    }

    public void DoPlayerInfo(CCSPlayerController player, ItemOption selectedOption)
    {
        Dndcs2.ShowDndXp(player, Utilities.GetPlayers().First(p => p.PlayerName == selectedOption.Text));
    }
    
}