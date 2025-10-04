using System.Drawing;
using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using static Dndcs2.messages.DndMessages;
using Dndcs2.stats;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Dndcs2.commands.SpellsAbilities;

public class GoodChicken : DndAbility
{
    public GoodChicken() : 
        base(
            new List<AbilityClassSpecieRequirement>()
            {
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(constants.DndClass.Druid),
            }, 
            5, 
            null, 
            .1,
            0,
            "!goodchicken", 
            "Spawns chickens your team can kill for health")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        var location = Dndcs2.GetViewLocation(player, distance: 1000, cutShortDistance: 50);
        if (location == null)
        {
            MessagePlayer(player, "Couldn't spawn the chicken, you have been given your mana back");
            return false;
        }
        
        Dndcs2.Instance.Log.LogInformation($"Original location {location}");

        location.Z += 50;
        
        CChicken? chicken = Utilities.CreateEntityByName<CChicken>("chicken");
        if (chicken == null)
        {
            chicken = Utilities.CreateEntityByName<CChicken>("chicken");
            if (chicken == null)
            {
                MessagePlayer(player, "Couldn't spawn the chicken, you have been given your mana back");
                return false;
            }
        }
        playerStats.Chickens.Add(chicken);
        chicken.Teleport(location, Vector3.Zero, Vector3.Zero);
        chicken.DispatchSpawn();
        Schema.SetSchemaValue(chicken.Handle, "CChicken", "m_leader", player.Pawn.Raw);
        Color color;
        if(player.Team == CsTeam.CounterTerrorist) 
            color = Color.FromName("blue");
        else 
            color = Color.FromName("red");

        Dndcs2.SetGlowing(chicken, color);
        
        
        return true;
    }
}