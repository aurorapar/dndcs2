using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class PlayerSpawn<T, TU, TV> : DndBaseEvent<T, TU, TV>
    where T : GameEvent
    where TU : GameEvent
    where TV : GameEvent
{
    public override HookResult PostHookCallback(TU e, GameEventInfo info)
    {
        var @event = ConvertEventType<EventPlayerSpawn>(e);
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");
        
        MessagePlayer(@event.Userid, "Welcome!");
        return HookResult.Continue;   
    }
}