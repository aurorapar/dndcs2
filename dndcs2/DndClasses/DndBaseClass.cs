using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.events;
using Dndcs2.dtos;
using Dndcs2.Sql;
using Dndcs2.stats;
using DndClass = Dndcs2.dtos.DndClass;
using static Dndcs2.messages.DndMessages;
using DndSpecie = Dndcs2.constants.DndSpecie;


namespace Dndcs2.DndClasses;

public abstract class DndBaseClass : DndClass
{
    public List<EventCallbackFeatureContainer> DndClassSpecieEvents { get; protected set; } = new();
    
    protected DndBaseClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        int dndClassId, string dndClassName, string dndClassDescription, 
        Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, dndClassId, dndClassName, dndClassDescription, 
            dndClassRequirements)
    {
        Dndcs2.Instance.Log.LogInformation($"Created class {GetType().Name}");
    }    
}
