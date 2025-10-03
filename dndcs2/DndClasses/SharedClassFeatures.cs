using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.commands.SpellsAbilities;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class SharedClassFeatures
{
    public static void AddFullCasterMana(int level, PlayerBaseStats playerStats, DndPlayer dndPlayer)
    {
        int mana = (level + 1) / 2 * 5 + 10;
        mana += level / 2 * 10;
        playerStats.ChangeMana(mana);
        playerStats.ChangeMaxMana(mana);
                
        MessagePlayer(Utilities.GetPlayerFromUserid(playerStats.Userid),
            $"You have {playerStats.Mana} mana for being a Level {level} {(constants.DndClass) dndPlayer.DndClassId} (Check current mana with !mana)");
    }

    public static void ShowSpells(CCSPlayerController player, DndPlayer dndPlayer, PlayerBaseStats playerStats)
    {
        var spellList = new List<string>();
        foreach (var spellAbilityKvp in DndAbility.DndAbilities)
        {
            var ability = spellAbilityKvp.Value;
            if (ability.CheckClassSpecieRequirements(player) && !ability.IsCastingWithSpecie(playerStats, player) && ability.CommandName != "!abilities")
                spellList.Add(
                    $"{ability.CommandName.Replace("!spells", "!spells/!abilities")} - "
                    + (ability.ManaCost > 0 ? ability.ManaCost.ToString() : "No") + " Mana" 
                    + (ability.SpecieLimitedUses != null ? $"/{ability.SpecieLimitedUses?.ToString()}" + " daily uses " : "")
                    + " - "
                    + (ability.LimitedUses != null ? $"{ability.LimitedUses?.ToString()}" + " uses " : "")
                    + ability.CommandDescription);                
        }

        if (spellList.Any())
        {
            MessagePlayer(player,
                $"Your {(constants.DndClass)dndPlayer.DndClassId} Spell List (check with !spells): ");
            foreach (var spell in spellList)
                MessagePlayer(player, spell);
        }
    }
}