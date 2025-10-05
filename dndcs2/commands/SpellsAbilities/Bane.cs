using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.stats;

namespace Dndcs2.commands.SpellsAbilities;

public class Bane : DndAbility
{
    public Bane() : 
        base(
            "Bane",
            new List<AbilityClassSpecieRequirement>()
            {
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(constants.DndClass.Cleric, 3),
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(constants.DndSpecie.Tiefling),
            }, 
            10, 
            null, 
            .1,
            1,
            "!bane", 
            "Curses someones stats with a subtracted 1d4 per roll")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        var target = Dndcs2.GetViewPlayer(player);
        if (target == null)
            return false;
        
        if (target.Team == player.Team || !target.PawnIsAlive)
            return false;

        var targetStats = PlayerStats.GetPlayerStats(target);
        if (targetStats.Bane)
        {
            MessagePlayer(player, "The target has already been cursed with Bane");
            return false;
        }

        targetStats.Bane = true;
        MessagePlayer(player, $"You have cursed {target.PlayerName} with Bane");
        target.EmitSound("SprayCan.ShakeGhost");
        MessagePlayer(target, $"{player.PlayerName} cursed you with Bane");
        
        return true;
    }
}