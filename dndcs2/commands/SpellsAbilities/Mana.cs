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
        Hidden = true;
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        var message = "";
        if (playerStats.MaxMana < 1 || !player.PawnIsAlive)
            message = "You have no mana.";
        else
        {
            char manaCharacter = '✨';
            char missingManaCharacter = '.';
            int manaPerBar = 20;
            
            int maxMana = playerStats.MaxMana;
            int currentMana = playerStats.Mana;
            double currentManaPercent = (double)currentMana / maxMana * 100;
            string manaColor = "lime";
            if (50 < currentManaPercent && currentManaPercent < 70)
                manaColor = "yellow";
            if (30 < currentManaPercent && currentManaPercent < 50)
                manaColor = "orange";
            if (currentManaPercent <= 30)
                manaColor = "red";
            if (currentMana == 0)
                message += "You have no mana!<br>";
            else
                message += $"<font color=\"{manaColor}\">[{currentMana}/{maxMana}] {(double)currentMana / maxMana * 100:0}%</font><br>";

            List<string> bars = new();
            while (currentMana > 0)
            {
                int barSize = Math.Min(manaPerBar, currentMana);
                currentMana -= barSize;
                maxMana -=  barSize;
                bars.Add(new String(manaCharacter, barSize));                
            }

            // if (bars.Last().Length < manaPerBar)
            // {
            //     var missingManaLength = Math.Min(manaPerBar, maxMana);
            //     missingManaLength = Math.Min(missingManaLength, manaPerBar - bars.Last().Length);
            //     maxMana -= missingManaLength;
            //     var previousBar = bars.Last() + new String(missingManaCharacter, missingManaLength);
            //     bars.Remove(bars.Last());
            //     bars.Add(previousBar);
            // }
            // while (maxMana > 0)
            // {
            //     int barSize = Math.Min(manaPerBar, maxMana);
            //     maxMana -=  barSize;
            //     bars.Add(new String(missingManaCharacter, barSize));
            // }

            foreach (var bar in bars.Take(Math.Min(4, bars.Count)))
                message += $"[{bar}]<br>";
        }
        
        player.PrintToCenterHtml(message); 
        return true;
    }
}