using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using Dndcs2.stats;

namespace Dndcs2.commands.SpellsAbilities;

public class Guidance : DndAbility
{
    public Guidance() : 
        base(
            new List<AbilityClassSpecieRequirement>()
            {
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(constants.DndClass.Cleric),
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(constants.DndSpecie.Aasimar),
            }, 
            5, 
            null, 
            .1,
            1,
            "!guidance", 
            "Buffs someones stats with an added 1d4 per roll")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        var target = Dndcs2.GetViewPlayer(player);
        if (target == null)
            return false;
        
        if (target.Team != player.Team || !target.PawnIsAlive)
            return false;

        var targetStats = PlayerStats.GetPlayerStats(target);
        if (targetStats.Guidance)
        {
            MessagePlayer(player, "The target has already been blessed with Guidance");
            return false;
        }

        targetStats.Guidance = true;
        if (player != target)
        {
            MessagePlayer(player, $"You have blessed {target.PlayerName} with Guidance");
            MessagePlayer(target, $"{player.PlayerName} blessed you with Guidance");
        }
        else
        {
            MessagePlayer(player, $"You have been blessed with Guidance");
        }

        return true;
    }
}