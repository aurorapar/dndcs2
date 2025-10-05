using CounterStrikeSharp.API;

namespace Dndcs2.timers;

public class GenericTimer : DndTimer
{
    private Action _callback;
    public GenericTimer(float duration, float frequency, int iterations, Action fireEvent) : 
        base(duration, frequency, iterations)
    {
        _callback = fireEvent;
    }

    public override void Fire()
    {
        _callback();
    }
}