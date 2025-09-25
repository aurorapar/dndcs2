using System.ComponentModel.DataAnnotations;

namespace Dndcs2.dtos;

public class DndClassProgress : MetaDtoObject
{
    [Key]
    public int DndClassExperienceId { get; private set; }
    public int DndPlayerId { get; private set; }
    public int DndClassId { get; private set; }
    public int DndExperienceAmount { get; set; }
    public int DndLevelAmount { get; set; }
    
    public DndClassProgress(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, 
        bool enabled, int dndClassExperienceId, int dndPlayerId, int dndClassId, int dndLevelAmount = 0, 
        int dndExperienceAmount = 0) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndClassExperienceId = dndClassExperienceId;
        DndPlayerId = dndPlayerId;
        DndClassId = dndClassId;
        DndExperienceAmount = dndExperienceAmount;
        DndLevelAmount = dndLevelAmount;
    }

    public DndClassProgress(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate,
        bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        
    }
}