using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;

namespace Dndcs2.events;

public abstract class DndEvent<T> : DndEventContainer
    where T : GameEvent
{
    public delegate HookResult EventCallback(T gameEvent, GameEventInfo info);
    public List<DndClassSpecieEventFeatureContainer> PreEventCallbacks { get; private set; } = new();
    public List<DndClassSpecieEventFeatureContainer> PostEventCallbacks { get; private set; } = new();

    public DndEvent()
    {
        Dndcs2.Instance.RegisterEventHandler<T>((@event, info) =>
        {
            return DefaultPreHookCallback(@event, info);
        }, HookMode.Pre);
        
        Dndcs2.Instance.RegisterEventHandler<T>((@event, info) =>
        {
            return DefaultPostHookCallback(@event, info);
        }, HookMode.Post);
    }
    
    public virtual HookResult DefaultPreHookCallback(T @event, GameEventInfo info)
    {
        return HookResult.Continue;
    }
    
    public virtual HookResult DefaultPostHookCallback(T @event, GameEventInfo info)
    {
        return HookResult.Continue;   
    }
}