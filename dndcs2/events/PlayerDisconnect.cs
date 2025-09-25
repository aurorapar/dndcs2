using CounterStrikeSharp.API.Core;
using Dndcs2.Sql;
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
        
        BroadcastMessage($"See ya later, {@event.Userid.PlayerName}!");
        
        var dndPlayer = CommonMethods.RetrievePlayer(@event.Userid);
        CommonMethods.TrackPlayerLogout(dndPlayer, GetType().Name);
        
        return HookResult.Continue;
    }
}
