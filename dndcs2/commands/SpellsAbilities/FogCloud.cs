using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.stats;

namespace Dndcs2.commands.SpellsAbilities;

public class FogCloud : DndAbility
{
    public FogCloud() : 
        base(
            "Fog Cloud",
            new List<AbilityClassSpecieRequirement>()
            {
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(constants.DndClass.Wizard),
            }, 
            5, 
            null, 
            .1,
            0,
            "!fogcloud", 
            "Creates a smoke effect to obscure sight")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        var target = Dndcs2.GetViewLocation(player);
        if (target == null)
            return false;

        var location = new Vector(target.X, target.Y, target.Z);
        Dndcs2.SpawnSmokeGrenade(location, QAngle.Zero, Vector.Zero, player.Team, true);
        
        return true;
    }
}