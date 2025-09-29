using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using Dndcs2.dtos;
using Dndcs2.events;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2;

public partial class Dndcs2
{
    public List<DndEventContainer> DndEvents { get; private set; } = new();
    public void RegisterEventCallbacks()
    {
        DndEvents = new List<DndEventContainer>()
        {
            new PlayerDeath(),
            new PlayerHurt(),
            new PlayerConnectFull(),
            new PlayerDisconnect(),
            new PlayerSpawn(),
            new RoundStart(),
            new RoundStartPreEntity(),
            new RoundEnd(),
            new BombDefused(),
            new BombExplode(),
            new BombPlant(),
            new HostageKilled(),
            new HostageRescued(),
        };
    }
}

public abstract class DndEvent<T> : DndEventContainer
    where T : GameEvent
{
    public delegate HookResult EventCallback(T gameEvent, GameEventInfo info);
    public List<EventCallbackFeatureContainer> PreEventCallbacks { get; private set; } = new();
    public List<EventCallbackFeatureContainer> PostEventCallbacks { get; private set; } = new();

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
        
        Dndcs2.Instance.Log.LogInformation($"Created Event {GetType().Name}");
    }

    public HookResult InnerPrehookCallback(T @event, GameEventInfo info)
    {
        Dndcs2.Instance.Log.Debug($"Event {GetType().Name} fired prehook");
        try
        {
            return DefaultPreHookCallback(@event, info);
        }
        catch (Exception e)
        {
            Dndcs2.Instance.Log.LogError($"Error occurred trying to fire prehook for {GetType().Name}\n\t{e}");    
        }
        return HookResult.Continue;
    }
    
    public HookResult InnerPosthookCallback(T @event, GameEventInfo info)
    {
        Dndcs2.Instance.Log.Debug($"Event {GetType().Name} fired posthook");
        try
        {
            return DefaultPostHookCallback(@event, info);
        }
        catch (Exception e)
        {
            Dndcs2.Instance.Log.LogError($"Error occurred trying to fire posthook for {GetType().Name}\n\t{e}");    
        }
        return HookResult.Continue;
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
        if (!roundStartEvent.XpRoundTracker.ContainsKey(dndPlayer.DndPlayerId))
            roundStartEvent.XpRoundTracker[dndPlayer.DndPlayerId] = new();
        roundStartEvent.XpRoundTracker[dndPlayer.DndPlayerId].Add(xpLogItem);
    }
}