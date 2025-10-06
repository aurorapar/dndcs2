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
            new DruidSpawn(),
            new Wildshape(),
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
    
    public class Wildshape : EventCallbackFeature<EventPlayerHurt>
    {
        public Wildshape() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Pre, PlayerHurtPre, 
                constants.DndClass.Druid, null)
        {
            
        }

        public static HookResult PlayerHurtPre(EventPlayerHurt @event, GameEventInfo info, DndPlayer dndPlayerVictim,
            DndPlayer dndPlayerAttacker)
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;
            var attackerClassEnum = (constants.DndClass) dndPlayerAttacker.DndClassId;
            var victimClassEnum = (constants.DndClass) dndPlayerVictim.DndClassId;

            if (attackerClassEnum != constants.DndClass.Druid && victimClassEnum != constants.DndClass.Druid)
                return HookResult.Continue;

            if (attacker.Team == victim.Team)
                return HookResult.Continue;
            
            var attackerStats = PlayerStats.GetPlayerStats(attacker);
            if (attackerStats.Wildshape)
            {
                if (attacker.PlayerPawn.Value.Health <= attackerStats.WildshapeHealth - 100)
                    Dewildshape(attacker, attackerStats, dndPlayerAttacker);
                
                var weapon = Dndcs2.GetPlayerWeapon(attacker);
                if (weapon != null && weapon.Contains("knife"))
                    Dndcs2.UpdatePrehookDamage(@event, (int) (@event.DmgHealth * 1.5));
            }
            
            var victimStats = PlayerStats.GetPlayerStats(victim);
            if (victimStats.Wildshape)
            {
                if (victim.PlayerPawn.Value.Health <= victimStats.WildshapeHealth - 100)
                    Dewildshape(victim, victimStats, dndPlayerVictim);
            }
            
            return HookResult.Changed;
        }
    }

    private static void Dewildshape(CCSPlayerController player, PlayerBaseStats playerStats, DndPlayer dndPlayer)
    {
        var playerClass = Dndcs2.Instance.DndClassLookup[(constants.DndClass) dndPlayer.DndClassId];
        var classLevel = CommonMethods.RetrievePlayerClassLevel(player);
        var originalHealth = (int)playerStats.GetPlayerHealthPerLevel(playerClass.HealthRating) * classLevel;
        playerStats.ChangeMaxHealth(originalHealth);        
        playerStats.ChangeSpeed(-.3);
        playerStats.PermitWeapons(playerClass.WeaponList);
        playerStats.Wildshape = false;
        player.PlayerPawn.Value.SetModel(playerStats.OriginalModel);
        MessagePlayer(player, "You have come out of Wildshape for taking sufficient damage!");
    }    
}