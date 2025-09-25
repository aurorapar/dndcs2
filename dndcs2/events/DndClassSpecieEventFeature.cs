using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using Dndcs2.constants;
    
namespace Dndcs2.events;

public abstract class DndClassSpecieEventFeature<T> : DndClassSpecieEventFeatureContainer
    where T : GameEvent
{
    public DndEvent<T> BaseEvent { get; private set; }
    public bool OverrideDefaultBehavior { get; private set; }
    public DndClassSpecieEventPriority Priority { get; private set; }
    public HookMode @HookMode { get; private set; }
    public Func<T, GameEventInfo, HookResult> Callback { get; private set; }
    public DndClass DndClass { get; private set; }
    public DndSpecie DndSpecie { get; private set; }

    public DndClassSpecieEventFeature(bool overrideDefaultBehavior, DndClassSpecieEventPriority priority, HookMode hookMode, 
        Func<T, GameEventInfo, HookResult> callback, DndClass? dndClass = null, DndSpecie? dndSpecie = null)
    {
        if (dndClass == null && dndSpecie == null)
            throw new Exception($"Do not have an overriding class or specie behavior for {typeof(T).Name}");

        OverrideDefaultBehavior = overrideDefaultBehavior;
        Priority = priority;
        @HookMode = hookMode;
        Callback = callback;
        if (dndClass != null)
            DndClass = (DndClass) dndClass;
        else if (dndSpecie != null)
            DndSpecie = (DndSpecie) dndSpecie;

        BaseEvent = DetermineBaseEvent();
        AddClassSpecieFeatureEvent();        
    }

    private DndEvent<T> DetermineBaseEvent()
    {
        return DndEvent<T>.RetrieveEvent<T>();
    }

    private void AddClassSpecieFeatureEvent()
    {
        if(HookMode == HookMode.Pre)
            BaseEvent.PreEventCallbacks.Add(this);
        if(HookMode == HookMode.Post)
            BaseEvent.PostEventCallbacks.Add(this);
    }
}

