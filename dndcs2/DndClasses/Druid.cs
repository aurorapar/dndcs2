using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using static Dndcs2.DndClasses.SharedClassFeatures;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Druid : DndBaseClass
{
    public override PlayerStat GoodStat { get; } = PlayerStat.Wisdom;
    public override PlayerStat AverageStat { get; } = PlayerStat.Intelligence;
    public override PlayerStatRating HealthRating { get; }= PlayerStatRating.Average;

    public override List<string> WeaponList { get; } = Dndcs2.Weapons
        .Except(Dndcs2.Snipers)
        .Except(Dndcs2.Rifles)
        .Except(Dndcs2.MGs)
        .Concat(new List<string>(){"galilar", "famas"})
        .ToList();
    
    public Druid(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndClass.Druid,new Collection<DndClassRequirement>())
    {
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new DruidSpawn()
        });
    }
    
    public class DruidSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public DruidSpawn() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                constants.DndClass.Druid, null)
        {
            
        }

        public static HookResult PlayerPostSpawn(EventPlayerSpawn @event, GameEventInfo info, DndPlayer dndPlayer,
            DndPlayer? dndPlayerAttacker)
        {
            var userid = (int) @event.Userid.UserId;
            Server.NextFrame(() =>
            {
                var player = Utilities.GetPlayerFromUserid(userid);
                if (player == null)
                    return;
                var dndPlayer = CommonMethods.RetrievePlayer(player);
                if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Druid)
                    return;

                var playerStats = PlayerStats.GetPlayerStats(dndPlayer);
                var druidLevel = CommonMethods.RetrievePlayerClassLevel(player);

                AddFullCasterMana(druidLevel, playerStats, dndPlayer);  
            });            
            return HookResult.Continue;
        }
    }
}