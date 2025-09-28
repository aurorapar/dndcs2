using System.Collections.ObjectModel;
using Dndcs2.events;
using Dndcs2.dtos;
using DndClass = Dndcs2.dtos.DndClass;


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
        
    }
}
