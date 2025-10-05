using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Menu;
using Dndcs2.DndClasses;
using Dndcs2.dtos;
using Dndcs2.Sql;
using Dndcs2.stats;
using DndClass = Dndcs2.constants.DndClass;
using DndSpecie = Dndcs2.constants.DndSpecie;

namespace Dndcs2.menus;

public class SpellMenu : PlayerMenu
{
    private Dictionary<string, Tuple<string, string, string>> _spellBook = new();
    private List<string> _hiddenAbilities = new List<string>()
    {
        "!mana",
        "!spells",
        "!abilities"
    };
    
    public SpellMenu(CCSPlayerController player, DndPlayer dndPlayer, PlayerBaseStats playerStats) : base ("[Spellbook]", Dndcs2.Instance)
    {
        _spellBook = SharedClassFeatures.ShowSpells(player, dndPlayer, playerStats);
        
        ExitButton = true;
        foreach (var spell in _spellBook)
        {
            if (_hiddenAbilities.Contains(spell.Key))
                continue;
            AddItem(spell.Key, (player, option) => DisplaySpellInformation(player, option));
        }
    }

    public void DisplaySpellInformation(CCSPlayerController player, ItemOption selectedOption)
    {
        MenuManager.CloseActiveMenu(player);
        Server.NextFrame(() =>
        {
            string spellInformation = "<font color=\"lime\">" + selectedOption.Text + "</font> - " + _spellBook[selectedOption.Text].Item1 + "<br>";
            spellInformation += _spellBook[selectedOption.Text].Item2 + "<br>";
            spellInformation += _spellBook[selectedOption.Text].Item3 + "<br>";
            player.PrintToCenterHtml(spellInformation, 10);
        });
    }
}