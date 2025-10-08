using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dndcs2.dtos;

public class DndClassRequirement : MetaDtoObject
{
    [Key]
    public int DndClassRequirementId { get; private set; }
    [ForeignKey(nameof(DndClass.DndClassId))]
    public int DndPrincipleClassId { get; private set; }
    [ForeignKey(nameof(DndClass.DndClassId))]
    public int DndDependentClassId { get; private set; }
    public int DndRequiredClassLevel { get; private set; }

    public DndClassRequirement(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled,
        int dndPrincipleClassId, int dndDependentClassId, int dndRequiredClassLevel) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndPrincipleClassId = dndPrincipleClassId;
        DndDependentClassId = dndDependentClassId;
        DndRequiredClassLevel = dndRequiredClassLevel;
    }
}