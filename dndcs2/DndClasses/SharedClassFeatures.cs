using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.commands.SpellsAbilities;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class SharedClassFeatures
{
    public static void AddHalfCasterMana(int level, PlayerBaseStats playerStats, DndPlayer dndPlayer)
    {
        int mana = level * 5 + 5;
        playerStats.ChangeMana(mana);
        playerStats.ChangeMaxMana(mana);
                
        MessagePlayer(Utilities.GetPlayerFromUserid(playerStats.Userid),
            $"You have {playerStats.Mana} mana for being a Level {level} {(constants.DndClass) dndPlayer.DndClassId} (Check current mana with !mana)");
    }
    
    public static void AddFullCasterMana(int level, PlayerBaseStats playerStats, DndPlayer dndPlayer)
    {
        int mana = (level + 1) / 2 * 5 + 10;
        mana += level / 2 * 10;
        playerStats.ChangeMana(mana);
        playerStats.ChangeMaxMana(mana);
                
        MessagePlayer(Utilities.GetPlayerFromUserid(playerStats.Userid),
            $"You have {playerStats.Mana} mana for being a Level {level} {(constants.DndClass) dndPlayer.DndClassId} (Check current mana with !mana)");
    }

    public static Dictionary<string, Tuple<string, string, string>> ShowSpells(CCSPlayerController player, DndPlayer dndPlayer, PlayerBaseStats playerStats, bool print=false)
    {
        var spellBook = new Dictionary<string, Tuple<string, string, string>>();
        
        foreach (var spellAbilityKvp in DndAbility.DndAbilities)
        {
            var ability = spellAbilityKvp.Value;
            if(ability.Hidden)
                continue;
            
            if (ability.CheckClassSpecieRequirements(player))
            {
                var abilityCost = $"";
                if (ability.ManaCost > 0)
                {
                    if (ability.IsCastingWithSpecie(playerStats, player))
                        abilityCost = $"{ability.SpecieLimitedUses} Daily Uses ({(constants.DndSpecie)dndPlayer.DndSpecieId})";
                    else
                        abilityCost = $"{ability.ManaCost} Mana";                    
                }
                if(ability.LimitedUses.HasValue && ability.LimitedUses.Value > 0)
                    abilityCost += abilityCost.Length > 0 ? " - " : "" + $"{ability.LimitedUses.Value} Daily Uses";
                

                spellBook[ability.Name] = new Tuple<string, string, string>(
                    ability.CommandName,
                    abilityCost, 
                    ability.CommandDescription);
            }
        }

        if (spellBook.Any() && print)
        {
            MessagePlayer(player,
                $"Your {(constants.DndClass)dndPlayer.DndClassId} Spell List (check with !spells): ");
            foreach (var spell in spellBook)
            {
                string output = $"{spell.Key}";
                output += $" - {spell.Value.Item1}";
                if (spell.Value.Item1.Length > 0)
                    output += $" - {spell.Value.Item2}";
                output +=  $" - {spell.Value.Item3}";
                MessagePlayer(player, output);
            }
        }
        
        return spellBook;
    }
}