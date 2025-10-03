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

public class Cleric : DndBaseClass
{
    public override PlayerStat GoodStat { get; } = PlayerStat.Wisdom;
    public override PlayerStat AverageStat { get; } = PlayerStat.Charisma;
    public override PlayerStatRating HealthRating { get; }= PlayerStatRating.Average;

    public override List<string> WeaponList { get; } = Dndcs2.Weapons
        .Except(Dndcs2.Snipers)
        .Except(Dndcs2.Rifles)
        .Except(Dndcs2.MGs)
        .Concat(new List<string>(){"galilar", "famas"})
        .ToList();
    
    public Cleric(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndClass.Cleric,new Collection<DndClassRequirement>())
    {
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new ClericSpawn()
        });
    }
    
    public class ClericSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public ClericSpawn() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                constants.DndClass.Cleric, null)
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
                if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Cleric)
                    return;

                var playerStats = PlayerStats.GetPlayerStats(dndPlayer);
                var clericLevel = CommonMethods.RetrievePlayerClassLevel(player);

                AddFullCasterMana(clericLevel, playerStats, dndPlayer);  
            });            
            return HookResult.Continue;
        }
    }
}