using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;
using static Dndcs2.messages.DndMessages;
using DndClass = Dndcs2.constants.DndClass;
using DndSpecie = Dndcs2.constants.DndSpecie;

namespace Dndcs2.timers;

public abstract class DndTimer
{
    public float StartTime { get; private set; } = 0;
    public float Duration {get; private set;}
    public float Frequency {get; private set;}
    public int? Iterations { get; private set; }

    public DndTimer(float duration, float frequency, int? iterations)
    {
        Duration = duration;
        Frequency = frequency;
        Iterations = iterations;
        
        Dndcs2.Instance.RegisterListener<Listeners.OnTick>(TickEventListener);
        
        var erspeDndEvent = DndEvent<EventRoundStartPreEntity>.RetrieveEvent<EventRoundStartPreEntity>();
        erspeDndEvent.PostEventCallbacks.Add(
            new ListenerRemover(
                (@event, gameEventInfo, dndPlayer, notUsedDndPlayer) =>
                {
                    Dndcs2.Instance.RemoveListener<Listeners.OnTick>(TickEventListener);
                    return HookResult.Continue;
                }
            )
        );
    }

    public void Stop()
    {
        
    }

    public void TickEventListener()
    {
        StartTime += Server.TickInterval;
        if (StartTime % Frequency == 0)
        {
            Fire();
            if(Iterations.HasValue)
                Iterations--;
        }

        if (StartTime >= Duration || (Iterations.HasValue & Iterations <= 0))
            Stop();
    }

    public abstract void Fire();

    public class ListenerRemover : EventCallbackFeature<EventRoundStartPreEntity>
    {
        public ListenerRemover(Func<EventRoundStartPreEntity, GameEventInfo, DndPlayer, DndPlayer?, HookResult> callback) : 
            base(false, EventCallbackFeaturePriority.High, HookMode.Pre, callback, null, null)
        {
            
        }        
    }
}