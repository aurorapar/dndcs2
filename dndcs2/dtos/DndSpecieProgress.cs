using System.ComponentModel.DataAnnotations;

namespace Dndcs2.dtos;

public class DndSpecieProgress : MetaDtoObject
{
    [Key]
    public int DndSpecieExperienceId { get; private set; }
    public int DndPlayerId { get; private set; }
    public int DndSpecieId { get; private set; }
    public int DndExperienceAmount { get; set; }
    public int DndLevelAmount { get; set; }
    
    public DndSpecieProgress(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, 
        bool enabled, int dndSpecieExperienceId, int dndPlayerId, int dndSpecieId, int dndLevelAmount = 0, 
        int dndExperienceAmount = 0) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndSpecieExperienceId = dndSpecieExperienceId;
        DndPlayerId = dndPlayerId;
        DndSpecieId = dndSpecieId;
        DndExperienceAmount = dndExperienceAmount;
        DndLevelAmount = dndLevelAmount;
    }

    public DndSpecieProgress(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate,
        bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        
    }
}