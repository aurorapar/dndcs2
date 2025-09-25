using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dndcs2.dtos;

public class DndSpecieProgress : MetaDtoObject
{
    [Key]
    public int DndSpecieExperienceId { get; private set; }
    [ForeignKey(nameof(DndPlayer.DndPlayerId))]
    public int DndPlayerId { get; private set; }
    [ForeignKey(nameof(DndSpecie.DndSpecieId))]
    public int DndSpecieId { get; private set; }
    public int DndExperienceAmount { get; set; }
    public int DndLevelAmount { get; set; }
    
    public DndSpecieProgress(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, 
        bool enabled, int dndPlayerId, int dndSpecieId, int dndLevelAmount = 1, 
        int dndExperienceAmount = 0) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
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