using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.events;

public class PlayerSpawn : DndEvent<EventPlayerSpawn>
{

    public PlayerSpawn() : base()
    {
        
    }
    
    public override HookResult DefaultPreHookCallback(EventPlayerSpawn @event, GameEventInfo info)
    {         
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");        
        
        var userId = (int) @event.Userid.UserId;
        var playerStats = PlayerStats.GetPlayerStats(userId);
        playerStats.Reset();
        return HookResult.Continue;   
    }   
}