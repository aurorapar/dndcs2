using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using static Dndcs2.Dndcs2;
using static Dndcs2.messages.DndMessages;
namespace dndcs2.events;

public abstract class DndEvent<T, TU, TV>
    where T : GameEvent
    where TU : GameEvent
    where TV : GameEvent
{       
    public abstract HookResult PreHookCallback(T @event, GameEventInfo info);
    public abstract HookResult PostHookCallback(TU @event, GameEventInfo info);
    public abstract HookResult AnnounceHookCallback(TV @event, GameEventInfo info);

    public DndEvent()        
    {
        PrintMessageToConsole($"Event {typeof(T).Name} registering hooks");
        Instance.RegisterEventHandler<T>(PreHookCallback, HookMode.Pre);
        Instance.RegisterEventHandler<TU>(PostHookCallback, HookMode.Post);
        Instance.RegisterEventHandler<TV>(AnnounceHookCallback, HookMode.Post);
    }

    protected TW ConvertEventType<TW>(GameEvent @event) where TW : GameEvent
    {
        var e = @event as TW;
        if(e is null)
            throw new Exception($"Hook event of type {@event.GetType()} is not type of {typeof(T).Name}");
        return e;
    }
}

