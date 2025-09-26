using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using Dndcs2.dtos;
using Dndcs2.events;

namespace Dndcs2;

public partial class Dndcs2
{
    public List<DndEventContainer> DndEvents { get; private set; } = new();
    public void RegisterEventCallbacks()
    {
        DndEvents = new List<DndEventContainer>()
        {
            new PlayerDeath(),
            new PlayerConnectFull(),
            new PlayerDisconnect(),
            new PlayerSpawn(),
            new RoundStart(),
            new RoundEnd(),
            new BombDefused(),
            new BombExplode(),
            new BombPlant(),
            new HostageKilled(),
            new HostageRescued()
        };
    }
}

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

    public static DndEvent<TU> RetrieveEvent<TU>() where TU : GameEvent
    {
        DndEvent<TU>? baseEvent = null;
        foreach (var registeredEvent in Dndcs2.Instance.DndEvents)
        {
            if (registeredEvent is DndEvent<TU>)
            {
                baseEvent = registeredEvent as DndEvent<TU>;
                break;
            }
        }

        if (baseEvent is null)
            throw new Exception($"Could not find a base registered event for {nameof(TU)}");
        
        return baseEvent;
    }

    public static void GrantPlayerExperience(DndPlayer dndPlayer, int amount, string reason, string source)
    {
        var roundStartEvent = (RoundStart) RetrieveEvent<EventRoundStart>();
        var xpLogItem = new DndExperienceLog(source, DateTime.UtcNow, source, DateTime.UtcNow, true,
            dndPlayer.DndPlayerId, amount, reason);
        roundStartEvent.XpRoundTracker[dndPlayer.DndPlayerId].Add(xpLogItem);
    }
}