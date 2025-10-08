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
    [MaxLength(2000)]
    public string DndClassDescription { get; private set; }

    public ICollection<DndClassRequirement> DndClassRequirements { get; } = new List<DndClassRequirement>();

    public DndClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        int dndClassId, string dndClassName, string dndClassDescription, Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndClassId = dndClassId;
        DndClassName = dndClassName;
        DndClassRequirements = DndClassRequirements.Union(dndClassRequirements).ToList();
        DndClassDescription = dndClassDescription;
    }
    
    // This is for EF. Do not use
    private DndClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        
    }
      
}
