using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dndcs2.dtos;

public class DndPlayer : MetaDtoObject
{
    [Key]
    public int DndPlayerId { get; private set; }
    public int DndPlayerAccountId { get; private set; }
    public int DndGoldAmount { get; set; }
    [ForeignKey(nameof(DndClass.DndClassId))]
    [DefaultValue(1)]
    public int DndClassId { get; set; }
    [ForeignKey(nameof(DndSpecie.DndSpecieId))]
    [DefaultValue(1)]
    public int DndSpecieId { get; set; }
    public DateTime LastConnected { get; set; }
    public DateTime? LastDisconnected { get; set; }
    public TimeSpan? PlayTime { get; set; }
    [DefaultValue(0)]
    public int Warnings { get; set; }
    public ICollection<DndPlayerStats> DndPlayerStats { get; } = new List<DndPlayerStats>();
    public ICollection<DndClassProgress> DndClassExperiences { get; } = new List<DndClassProgress>();
    public ICollection<DndSpecieProgress> DndSpecieExperience { get; } = new List<DndSpecieProgress>();

    public DndPlayer(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        int dndPlayerAccountId, int dndGoldAmount, int dndClassId, int  dndSpecieId, DateTime lastConnected) : 
        base( createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndPlayerAccountId = dndPlayerAccountId;
        DndGoldAmount = dndGoldAmount;
        DndClassId = dndClassId;
        DndSpecieId = dndSpecieId;
        LastConnected = lastConnected;
    }

    // This is for EF. Do not use
    private DndPlayer(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        
    }
}
