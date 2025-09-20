using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dndcs2.dtos;

public class DndClass : MetaDtoObject
{
    [Key]
    public int DndClassId { get; private set; }
    [Required]
    [MaxLength(100)]
    public string DndClassName { get; private set; }
    public ICollection<DndClassRequirement> DndClassRequirements { get; } = new List<DndClassRequirement>();

    public DndClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        int dndClassId, string dndClassName, Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndClassId = dndClassId;
        DndClassName = dndClassName;
        DndClassRequirements = DndClassRequirements.Union(dndClassRequirements).ToList();
    }
    
    // This is for EF. Do not use
    private DndClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        
    }
      
}

public class DndClassRequirement : MetaDtoObject
{
    [Key]
    public int DndClassRequirementId { get; private set; }
    [ForeignKey(nameof(DndClass.DndClassId))]
    public int DndPrincipleClassId { get; private set; }
    [ForeignKey(nameof(DndClass.DndClassId))]
    public int DndRequiredClassId { get; private set; }
    public int DndRequiredClassLevel { get; private set; }

    public DndClassRequirement(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled,
        int dndPrincipleClassId, int dndRequiredClassId, int dndRequiredClassLevel) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndPrincipleClassId = dndPrincipleClassId;
        DndRequiredClassId = dndRequiredClassId;
        DndRequiredClassLevel = dndRequiredClassLevel;
    }
}