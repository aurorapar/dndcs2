using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using dndcs2.events;

namespace Dndcs2.events;

public class DndBaseEvent<T, TU, TV> : DndEvent<T, TU, TV>
    where T : GameEvent
    where TU : GameEvent
    where TV : GameEvent
{    
    public override HookResult PreHookCallback(T @event, GameEventInfo info)
    {
        return HookResult.Continue;
    }
    
    public override HookResult PostHookCallback(TU @event, GameEventInfo info)
    {
        return HookResult.Continue;   
    }

    public override HookResult AnnounceHookCallback(TV @event, GameEventInfo info)
    {
        return HookResult.Continue;
    }
}