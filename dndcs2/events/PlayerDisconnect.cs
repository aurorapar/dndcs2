using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using Dndcs2.Sql;
using Microsoft.Extensions.Logging;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class PlayerDisconnect<T, TU, TV> : DndBaseEvent<T, TU, TV>
where T : GameEvent
where TU : GameEvent
where TV : GameEvent
{            
    public override HookResult PostHookCallback(TU e, GameEventInfo info)
    {
        var @event = ConvertEventType<EventPlayerDisconnect>(e);

        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");
        
        BroadcastMessage($"See ya later, {@event.Userid.PlayerName}!");
        
        var accountId = new SteamID(@event.Userid.SteamID).AccountId;
        var dndPlayer = CommonMethods.RetrievePlayer(accountId);
        if (dndPlayer == null)
        {
            Dndcs2.DndLogger.LogError($"Player {accountId} not found when updating logout");
            return HookResult.Continue;    
        }
        CommonMethods.TrackPlayerLogout(dndPlayer, GetType().Name);
        
        return HookResult.Continue;
    }
}
