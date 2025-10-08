using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dndcs2.dtos;

public class DndSubClassProgress : MetaDtoObject
{
    [Key]
    public int DndClassExperienceId { get; private set; }
    [ForeignKey(nameof(DndPlayer.DndPlayerId))]
    public int DndPlayerId { get; private set; }
    [ForeignKey(nameof(DndClass.DndClassId))]
    public int DndSubClassId { get; private set; }
    public int DndExperienceAmount { get; set; }
    public int DndLevelAmount { get; set; }
    
    public DndSubClassProgress(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, 
        bool enabled, int dndPlayerId, int dndSubClassId, int dndLevelAmount = 1, 
        int dndExperienceAmount = 0) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndPlayerId = dndPlayerId;
        DndSubClassId = dndSubClassId;
        DndExperienceAmount = dndExperienceAmount;
        DndLevelAmount = dndLevelAmount;
    }

    public DndSubClassProgress(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate,
        bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        
    }
}