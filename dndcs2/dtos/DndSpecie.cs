using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dndcs2.dtos;

public class DndSpecie : MetaDtoObject
{
    [Key]
    public int DndSpecieId { get; private set; }
    [Required]
    [MaxLength(100)]
    public string DndSpecieName { get; private set; }
    [MaxLength(2000)]
    public string DndSpecieDescription { get; private set; }
    public int DndSpecieLevelAdjustment { get; private set; }
    public ICollection<DndSpecieRequirement> DndSpecieRequirements { get; } = new List<DndSpecieRequirement>();

    public DndSpecie(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        int dndSpecieId, string dndSpecieName, int dndSpecieLevelAdjustment, string dndSpecieDescription, 
        Collection<DndSpecieRequirement> dndSpecieRequirements) : 
        base( createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndSpecieId = dndSpecieId;
        DndSpecieName = dndSpecieName;
        DndSpecieLevelAdjustment = dndSpecieLevelAdjustment;
        DndSpecieRequirements = DndSpecieRequirements.Union(dndSpecieRequirements).ToList();
        DndSpecieDescription = dndSpecieDescription;
    }
    
    // This is for EF. Do not use
    private DndSpecie(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        
    }
}

public class DndSpecieRequirement : MetaDtoObject
{
    [Key]
    public int DndSpecieRequirementId { get; private set; }
    [ForeignKey(nameof(DndSpecie.DndSpecieId))]
    public int DndPrincipleSpecieId { get; private set; }
    [ForeignKey(nameof(DndSpecie.DndSpecieId))]
    public int DndRequiredSpecieId { get; private set; }
    public int DndRequiredSpecieLevel { get; private set; }

    public DndSpecieRequirement(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled,
        int dndPrincipleSpecieId, int dndRequiredSpecieId, int dndRequiredSpecieLevel) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndPrincipleSpecieId = dndPrincipleSpecieId;
        DndRequiredSpecieId = dndRequiredSpecieId;
        DndRequiredSpecieLevel = dndRequiredSpecieLevel;
    }
}