using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class PlayerSpawn : DndEvent<EventPlayerSpawn>
{

    public PlayerSpawn() : base()
    {
    
    }

    public override HookResult DefaultPostHookCallback(EventPlayerSpawn @event, GameEventInfo info)
    {         
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");
        
        MessagePlayer(@event.Userid, "Welcome!");
        return HookResult.Continue;   
    }
}