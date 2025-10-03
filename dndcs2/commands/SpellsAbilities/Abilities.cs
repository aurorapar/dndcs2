using CounterStrikeSharp.API.Core;
using Dndcs2.Sql;
using Dndcs2.stats;
using DndClass = Dndcs2.constants.DndClass;
using static Dndcs2.DndClasses.SharedClassFeatures;

namespace Dndcs2.commands.SpellsAbilities;

public class Abilities : DndAbility
{
    private static List<AbilityClassSpecieRequirement> _allClasses = Enum.GetValues(typeof(DndClass)).Cast<DndClass>()
        .ToList()
        .Select(c => ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(c))
        .ToList();
    public Abilities() : 
        base(
            _allClasses, 
            0, 
            null, 
            0,
            null,
            "!abilities", 
            "Allows user to check spell & ability list")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        ShowSpells(player, CommonMethods.RetrievePlayer(player), playerStats);
        return true;
    }
}