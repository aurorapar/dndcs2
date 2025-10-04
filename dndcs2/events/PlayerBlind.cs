using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.dice;
using Dndcs2.Sql;
using Dndcs2.stats;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class PlayerBlind : DndEvent<EventPlayerBlind>
{

    public PlayerBlind() : base()
    {
        
    }

    public override HookResult DefaultPreHookCallback(EventPlayerBlind @event, GameEventInfo info)
    {
        var victim = @event.Userid;
        if (!victim.PawnIsAlive)
            return HookResult.Stop;
        var eventAttacker = @event.Attacker;
        
        var flashbang = Utilities.GetEntityFromIndex<CFlashbangProjectile>(@event.Entityid);
        foreach (var playerStats in Utilities.GetPlayers().Select(p => PlayerStats.GetPlayerStats(p)))
        {
            if (playerStats.FlashbangLocation == null)
                continue;
            if (Server.TickCount - playerStats.FlashbangSpawnedTick > 32)
                continue;
            if (Vector3.Distance((Vector3)flashbang.AbsOrigin, (Vector3)playerStats.FlashbangLocation) > 20)
                continue;
            
            var attacker = Utilities.GetPlayerFromUserid(playerStats.Userid);
            
            if ((int)victim.Team == attacker.TeamNum)
            {
                Dndcs2.Instance.Log.LogInformation("Should have stopped event");
                Dndcs2.UnblindPlayer(victim);
                return HookResult.Stop;
            }
            
            var victimStats = PlayerStats.GetPlayerStats(victim);
            if (victimStats.MakeDiceCheck(attacker, PlayerStat.Intelligence, PlayerStat.Constitution))
            {
                Dndcs2.UnblindPlayer(victim);
                return HookResult.Stop;
            }

            return HookResult.Continue;
        }        
        
        return HookResult.Continue;
    }
}
