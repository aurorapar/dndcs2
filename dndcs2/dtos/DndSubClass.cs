
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Dndcs2.constants.DndSubClassDescription;

namespace Dndcs2.dtos;

public class DndSubClass : MetaDtoObject
{
    [Key]
    public int DndSubClassId { get; private set; }
    [Required]
    [MaxLength(100)]
    public string DndSubClassName { get; private set; }
    [MaxLength(2000)]
    public string DndSubClassDescription { get; private set; }
    [ForeignKey(nameof(DndClass.DndClassId))]
    public int DndParentClassId { get; set; }
    public int DndParentClassLevelRequirementId { get; set; }

    public DndSubClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        constants.DndSubClass dndSubClass, constants.DndClass dndParentClass, int dndParentClassLevelRequirementId = 3) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndSubClassId = (int) dndSubClass;
        DndSubClassName = Dndcs2.FormatConstantNameForDisplay(dndSubClass.ToString());
        DndSubClassDescription = DndSubClassDescriptions[dndSubClass];
        DndParentClassId = (int) dndParentClass;
        DndParentClassLevelRequirementId = dndParentClassLevelRequirementId;
    }
    
    // This is for EF. Do not use
    private DndSubClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        
    }
}
