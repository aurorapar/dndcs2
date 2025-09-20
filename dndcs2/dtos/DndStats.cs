using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dndcs2.dtos;

public class DndPlayerStats : MetaDtoObject
{
    [Key]
    public int DndPlayerStatsId { get; private set; }
    [ForeignKey(nameof(DndPlayer.DndPlayerId))]
    public int DndPlayerId { get; private set; }
    
    public int Kills = 0;
    public int Deaths = 0;
    public int Assists = 0;
    public int TeamKills = 0;
    public int TeamDeaths = 0;
    public int TeamAssists = 0;
    public int ShotsFired = 0;
    public int ShotsHit = 0;
    public int BombsPlanted = 0;
    public int BombsExploded = 0;
    public int BombsDefused = 0;
    public int HostagesRescued = 0;
    public int HostagesSecured = 0;
    public int HostagesKilled = 0;
    public int RoundsWon = 0;
    public int RoundsLost = 0;
    public int BotsKilled = 0;
    
    public DndPlayerStats(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled,
        int dndPlayerId) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled)
    {
        DndPlayerStatsId = dndPlayerId;
    }
}