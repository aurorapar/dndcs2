using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using Dndcs2.Sql;
using Microsoft.Extensions.Logging;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class PlayerDisconnect : DndEvent<EventPlayerDisconnect>
{

    public PlayerDisconnect() : base()
    {
    
    }

    public override HookResult DefaultPostHookCallback(EventPlayerDisconnect @event, GameEventInfo info)
    { 
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");        
        
        PrintMessageToConsole($"See ya later, {@event.Userid.PlayerName}! {@event.Userid.SteamID}");
        BroadcastMessage($"See ya later, {@event.Userid.PlayerName}! {@event.Userid.SteamID}");
        
        var dndPlayer = CommonMethods.RetrievePlayer(@event.Userid);
        
        CommonMethods.TrackPlayerLogout(dndPlayer, GetType().Name);
        
        return HookResult.Continue;
    }
}
