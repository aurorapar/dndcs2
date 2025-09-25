using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dndcs2.dtos;

public class DndExperienceLog : MetaDtoObject
{
    [Key]
    public int ExperienceLogId { get; private set; }
    [ForeignKey(nameof(DndPlayer.DndPlayerId))]
    public int DndPlayerId { get; private set; }
    public int ExperienceAmount { get; set; }
    
    
    public DndExperienceLog(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled,
    int dndPlayerId, int experienceAmount) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndPlayerId = dndPlayerId;
        ExperienceAmount = experienceAmount;
    }
    
    //For EF, don't use
    public DndExperienceLog(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) : base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        
    }
}
