using System.Collections.ObjectModel;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;
using DndClass = Dndcs2.constants.DndClass;

namespace Dndcs2.DndSpecies;

public class Dragonborn : DndBaseSpecie
{
    public Dragonborn(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        string dndSpecieName, string dndSpecieDescription, int dndSpecieLevelAdjustment, 
        Collection<DndSpecieRequirement> dndSpecieRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, (int) constants.DndSpecie.Dragonborn, dndSpecieName, 
            dndSpecieLevelAdjustment, dndSpecieDescription, dndSpecieRequirements)
    {        
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            
        });
        
    }    
}