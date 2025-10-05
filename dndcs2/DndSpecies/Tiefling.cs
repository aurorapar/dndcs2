using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.DndSpecies;

public class Tiefling : DndBaseSpecie
{
    public Tiefling(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndSpecie.Tiefling, 0,
            new Collection<DndSpecieRequirement>())
    {
        DndClassSpecieEvents.AddRange(new List<EventCallbackFeatureContainer>()
        {
            
        });
    }
}