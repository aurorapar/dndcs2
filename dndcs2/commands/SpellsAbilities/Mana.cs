using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.stats;
using DndClass = Dndcs2.constants.DndClass;

namespace Dndcs2.commands.SpellsAbilities;

public class Mana : DndAbility
{
    private static List<AbilityClassSpecieRequirement> _manaAbility = Enum.GetValues(typeof(constants.DndClass)).Cast<constants.DndClass>()
        .Select(c => ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(c)).ToList();
    public Mana() : 
        base(
            "",
            new List<AbilityClassSpecieRequirement>(_manaAbility), 
            0, 
            null, 
            0,
            null,
            "!mana", 
            "Allows user to get mana left")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        var message = "";
        if (playerStats.MaxMana < 1 || !player.PawnIsAlive)
            message = "You have no mana.";
        else
        {
            char manaCharacter = '=';
            char missingManaCharacter = '-';
            int manaPerBar = 30;
            
            int maxMana = playerStats.MaxMana;
            int currentMana = playerStats.Mana;
            if (currentMana == 0)
                message += "You have no mana!<br>";
            else
                message += $"[{currentMana}/{maxMana}] {(double)currentMana / maxMana * 100:0}%";
            int manaBars = Math.Min((int) Math.Ceiling((double) maxMana / manaPerBar), 1);
            for (int i = 0; i < manaBars; i++)
            {
                if (currentMana > 0)
                {
                    int manaInBar = Math.Min(currentMana, manaPerBar);
                    currentMana -= manaInBar;
                    message += "[" + new String(manaCharacter, manaInBar) + new String(missingManaCharacter, manaPerBar - manaInBar) + "]";
                }
                else
                {
                    message += "[" + new String(missingManaCharacter, manaPerBar) + "]";
                }

                message += "<br>";
            }            
        }
        
        player.PrintToCenterHtml(message); 
        return true;
    }
}