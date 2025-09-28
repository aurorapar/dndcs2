using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.timers;

public abstract class DndTimer
{
    public float Duration {get; private set;}
    public float Frequency {get; private set;}
    public int? Iterations { get; private set; }

    public DndTimer(float duration, float frequency, int? iterations)
    {
        Duration = duration;
        Frequency = frequency;
        Iterations = iterations;
        
        Dndcs2.Instance.RegisterListener<Listeners.OnTick>(TickEventListener);
        
        Dndcs2.Instance.RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            Stop();
            return HookResult.Continue;
        }, HookMode.Pre);
    }

    public void Stop()
    {
        Dndcs2.Instance.RemoveListener<Listeners.OnTick>(TickEventListener);
    }

    public void TickEventListener()
    {
        if (Iterations != null && Iterations < 1)
        {
            Stop();
            return;
        }

        if ((int)(Duration / Frequency) != (int)((Duration - Server.TickInterval) / Frequency))
            Fire();

        Duration -= Server.TickInterval;
        if(Duration <= 0)
            Stop();
    }

    public abstract void Fire();
}