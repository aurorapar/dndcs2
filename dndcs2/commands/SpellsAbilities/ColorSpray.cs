using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.stats;

namespace Dndcs2.commands.SpellsAbilities;

public class ColorSpray : DndAbility
{
    public ColorSpray() : 
        base(
            new List<AbilityClassSpecieRequirement>()
            {
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(constants.DndClass.Wizard),
            }, 
            5, 
            null, 
            .1,
            0,
            "!colorspray", 
            "Creates a Flashbang to blind enemies (Constitution save negates)")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        var target = Dndcs2.GetViewLocation(player);
        if (target == null)
            return false;

        playerStats.FlashbangLocation = new Vector(target.X, target.Y, target.Z);
        playerStats.FlashbangSpawnedTick = Server.TickCount;
        Dndcs2.SpawnFlashbang(playerStats.FlashbangLocation, QAngle.Zero, Vector.Zero, player.Team, true);
        
        return true;
    }
}