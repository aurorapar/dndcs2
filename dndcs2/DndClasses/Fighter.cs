using System.Collections.ObjectModel;
using Dndcs2.dtos;

namespace Dndcs2.DndClasses;

public class Fighter : DndBaseClass
{
    public Fighter(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        string dndClassName, string dndClassDescription, Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, (int) constants.DndClass.Fighter, 
            Enum.GetName(typeof(constants.DndClass), constants.DndClass.Fighter), 
            dndClassDescription, dndClassRequirements)
    {

    }
}