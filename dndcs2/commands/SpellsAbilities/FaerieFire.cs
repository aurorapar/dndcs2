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

public class FaerieFire : DndAbility
{
    public FaerieFire() : 
        base(
            "Faerie Fire",
            new List<AbilityClassSpecieRequirement>()
            {
                ClassSpecieAbilityRequirementFactory.ClassSpecieAbilityRequirement(constants.DndClass.Ranger),
            }, 
            10, 
            null, 
            .1,
            0,
            "!faeriefire", 
            "Causes the nearest enemy to glow with Fae Magik (Dex save negates)")
    {
        
    }

    public override bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments)
    {
        var location = player.AbsOrigin;
        var target = Utilities.GetPlayers()
            .Where(p => p.PawnIsAlive && p.Team != player.Team && !PlayerStats.GetPlayerStats(p).FaerieFire) 
            .OrderBy(p => Vector3.Distance((Vector3) p.AbsOrigin, (Vector3) player.AbsOrigin))
            .First();

        if (target == null)
        {
            MessagePlayer(player, "There were no enemies to track");
            return false;
        }

        if (PlayerStats.GetPlayerStats(target).MakeDiceCheck(player, PlayerStat.Dexterity, PlayerStat.Dexterity, false))
        {
            MessagePlayer(player, "No one was caught in the Faerie Fire");
        }
        else
        {
            Color color;
            if(target.Team == CsTeam.CounterTerrorist) 
                color = Color.FromName("blue");
            else 
                color = Color.FromName("red");

            Dndcs2.SetGlowing(target, color);
            MessagePlayer(target, "You are glowing brightly!");
            MessagePlayer(player, "You are tracking your quarry...");
        }
        
        return true;
    }
}