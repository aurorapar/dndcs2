using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using static Dndcs2.DndClasses.SharedClassFeatures;
using static Dndcs2.messages.DndMessages;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Ranger : DndBaseClass
{
    public override PlayerStat GoodStat { get; } = PlayerStat.Dexterity;
    public override PlayerStat AverageStat { get; } = PlayerStat.Strength;
    public override PlayerStatRating HealthRating { get; } = PlayerStatRating.High;

    public override List<string> WeaponList { get; } = Dndcs2.Weapons
        .Except(Dndcs2.Snipers)
        .Except(Dndcs2.MGs)
        .ToList();
    
    public Ranger(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndClass.Ranger,new Collection<DndClassRequirement>())
    {
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new RangerSpawn(),
            new ZephyrStrike(),            
        });
    }
    public class ZephyrStrike : EventCallbackFeature<EventPlayerHurt>
    {
        public ZephyrStrike() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Pre, PlayerHurtPre, 
                constants.DndClass.Fighter, null)
        {
            
        }

        public static HookResult PlayerHurtPre(EventPlayerHurt @event, GameEventInfo info, DndPlayer dndPlayerVictim,
            DndPlayer dndPlayerAttacker)
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;
            var attackerClassEnum = (constants.DndClass) dndPlayerAttacker.DndClassId;
            
            if(attackerClassEnum != constants.DndClass.Ranger )
                return HookResult.Continue;
            
            if (attacker.Team == victim.Team)
                return HookResult.Continue;
            
            var playerStats = PlayerStats.GetPlayerStats(attacker);
            playerStats.ChangeSpeed(.3, 5);
            return HookResult.Changed;
        }
    }
    
    
    public class RangerSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public RangerSpawn() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                constants.DndClass.Ranger, null)
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
                if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Ranger)
                    return;

                var playerStats = PlayerStats.GetPlayerStats(dndPlayer);
                var rangerLevel = CommonMethods.RetrievePlayerClassLevel(player);

                AddHalfCasterMana(rangerLevel, playerStats, dndPlayer);
                MessagePlayer(player, "Zephyr Strike: Your attacks increase your speed for a short amount of time");
            });            
            return HookResult.Continue;
        }
    }
}