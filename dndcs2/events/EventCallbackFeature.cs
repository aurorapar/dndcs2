using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using static Dndcs2.messages.DndMessages;
using Dndcs2.constants;
using Dndcs2.dtos;
using DndClass = Dndcs2.constants.DndClass;
using DndSpecie = Dndcs2.constants.DndSpecie;

namespace Dndcs2.events;

public abstract class EventCallbackFeature<T> : EventCallbackFeatureContainer
    where T : GameEvent
{
    public DndEvent<T> BaseEvent { get; private set; }
    public bool OverrideDefaultBehavior { get; private set; }
    public EventCallbackFeaturePriority CallbackFeaturePriority { get; private set; }
    public HookMode @HookMode { get; private set; }
    public Func<T, GameEventInfo, DndPlayer, DndPlayer?, HookResult> Callback { get; protected set; }
    public DndClass? DndClass { get; private set; }
    public DndSpecie? DndSpecie { get; private set; }
    private static List<string> AllowedNullEvents = new List<string>()
    {
        "PlayerBaseStatResetter",
        "BaseStats"
    };

    public EventCallbackFeature(bool overrideDefaultBehavior, EventCallbackFeaturePriority callbackFeaturePriority,
        HookMode hookMode,
        Func<T, GameEventInfo, DndPlayer, DndPlayer?, HookResult> callback, DndClass? dndClass = null, DndSpecie? dndSpecie = null)
    {
        if (dndClass == null && dndSpecie == null 
             && !AllowedNullEvents.Contains(GetType().Name)
        )
            throw new Exception($"Do not have an overriding class or specie behavior for {GetType().Name}");

        OverrideDefaultBehavior = overrideDefaultBehavior;
        CallbackFeaturePriority = callbackFeaturePriority;
        @HookMode = hookMode;
        Callback = callback;
        if (dndClass != null)
            DndClass = (DndClass) dndClass;
        else if (dndSpecie != null)
            DndSpecie = (DndSpecie) dndSpecie;

        BaseEvent = DetermineBaseEvent();
        AddClassSpecieFeatureEvent();        
        Dndcs2.Instance.Log.LogInformation($"Registed EventCallbackFeature {GetType().Name}");
    }

    private DndEvent<T> DetermineBaseEvent()
    {
        return DndEvent<T>.RetrieveEvent<T>();
    }

    private void AddClassSpecieFeatureEvent()
    {
        if (HookMode == HookMode.Pre)        
            BaseEvent.PreEventCallbacks.Add(this);

        if (HookMode == HookMode.Post)
            BaseEvent.PostEventCallbacks.Add(this);
        
        Dndcs2.Instance.Log.Debug($"Added {GetType().Name}");
    }
}

