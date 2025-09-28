using System.Collections.ObjectModel;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.events;
using Dndcs2.dtos;
using DndClass = Dndcs2.dtos.DndClass;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.DndClasses;

public abstract class DndBaseClass : DndClass
{
    public List<DndClassSpecieEventFeatureContainer> DndClassSpecieEvents { get; protected set; } = new();
    
    protected DndBaseClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        int dndClassId, string dndClassName, string dndClassDescription, 
        Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, dndClassId, dndClassName, dndClassDescription, 
            dndClassRequirements)
    {
        DndClassSpecieEvents.Add(
            new BaseStats()
        );
    }

    public class BaseStats : DndClassSpecieEventFeature<EventPlayerSpawn>
    {
        public BaseStats() :
            base(false, DndClassSpecieEventPriority.Medium, HookMode.Post, SetBaseStats,
                null, null)
        {
            
        }

        public static HookResult SetBaseStats(EventPlayerSpawn @event, GameEventInfo info, DndPlayer dndPlayerVictim, DndPlayer? notUsed)
        {
            return HookResult.Continue;
        }
    }
}
