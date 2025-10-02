using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.stats;
using DndClass = Dndcs2.constants.DndClass;

namespace Dndcs2.commands.SpellsAbilities;

public class Mana : DndAbility
{
    public Mana() : 
        base(
            new List<AbilityClassSpecieRequirement>()
            {
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(DndClass.Cleric),
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(DndClass.Wizard),
            }, 
            0, 
            null, 
            0,
            "!mana", 
            "Allows user to get mana left")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        MessagePlayer(player, $"You have {playerStats.Mana} mana");
        return true;
    }
}