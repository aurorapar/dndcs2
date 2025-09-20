using System;
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
    public int DndClassId { get; set; }
    [ForeignKey(nameof(DndSpecie.DndSpecieId))]
    public int DndSpecieId { get; set; }
    public DateTime LastConnected { get; set; }
    public DateTime? LastDisconnected { get; set; }
    public TimeSpan? PlayTime { get; set; }
    [DefaultValue(0)]
    public int Warnings { get; set; }

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

public class DndPlayerClassExperience : MetaDtoObject
{
    [Key]
    public int DndPlayerClassExperienceId { get; private set; }
    [ForeignKey(nameof(DndPlayer.DndPlayerId))]
    public int DndPlayerId { get; private set; }
    [ForeignKey(nameof(DndClass.DndClassId))]
    public int DndClassId { get; private set; } 
    public int DndExperienceAmount { get; private set; }

    public DndPlayerClassExperience(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate,
        bool enabled, int dndPlayerId, int dndClassId, int dndExperienceAmount) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndPlayerId = dndPlayerId;
        DndClassId = dndClassId;
        DndExperienceAmount = dndExperienceAmount;
    }
}

public class DndPlayerSpecieExperience : MetaDtoObject
{
    [Key]
    public int DndPlayerSpecieExperienceId { get; private set; }
    [ForeignKey(nameof(DndPlayer.DndPlayerId))]
    public int DndPlayerId { get; private set; }
    [ForeignKey(nameof(DndClass.DndClassId))]
    public int DndSpecieId { get; private set; } 
    public int DndExperienceAmount { get; private set; }

    public DndPlayerSpecieExperience(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate,
        bool enabled, int dndPlayerId, int dndSpecieId, int dndExperienceAmount) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndPlayerId = dndPlayerId;
        DndSpecieId = dndSpecieId;
        DndExperienceAmount = dndExperienceAmount;
    }
}