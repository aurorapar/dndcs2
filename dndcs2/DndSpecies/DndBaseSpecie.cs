using System.Collections.ObjectModel;
using Dndcs2.events;
using Dndcs2.dtos;
using Dndcs2.stats;

namespace Dndcs2.DndSpecies;

public abstract class DndBaseSpecie : DndSpecie
{
    public List<EventCallbackFeatureContainer> DndClassSpecieEvents { get; protected set; } = new();

    protected DndBaseSpecie(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled,
        int dndSpecieId, string dndSpecieName, int dndSpecieLevelAdjustment, string dndSpecieDescription,
        Collection<DndSpecieRequirement> dndSpecieRequirements) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, dndSpecieId, dndSpecieName,
            dndSpecieLevelAdjustment,
            dndSpecieDescription, dndSpecieRequirements)
    {
        
    }
}