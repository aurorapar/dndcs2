using CounterStrikeSharp.API.Core;
using Dndcs2.menus;
using Dndcs2.Sql;
using Dndcs2.stats;
using DndClass = Dndcs2.constants.DndClass;
using static Dndcs2.DndClasses.SharedClassFeatures;

namespace Dndcs2.commands.SpellsAbilities;

public class Spells : DndAbility
{
    private static List<AbilityClassSpecieRequirement> _allClasses = Enum.GetValues(typeof(DndClass)).Cast<DndClass>()
        .ToList()
        .Select(c => ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(c))
        .ToList();
    public Spells() : 
        base(
            "",
            _allClasses, 
            0, 
            null, 
            0,
            null,
            "!spells", 
            "Allows user to check spell & ability list")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        var spellbook = new SpellMenu(player, CommonMethods.RetrievePlayer(player), playerStats);
        spellbook.Display(player, 10);
        return true;
    }
}