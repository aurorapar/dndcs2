using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using static Dndcs2.messages.DndMessages;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Monk : DndBaseClass
{
    public override PlayerStat GoodStat { get; } = PlayerStat.Dexterity;
    public override PlayerStat AverageStat { get; } = PlayerStat.Strength;
    public override PlayerStatRating HealthRating { get; }= PlayerStatRating.Average;

    public override List<string> WeaponList { get; } = Dndcs2.Weapons
        .Except(Dndcs2.Snipers)
        .Except(Dndcs2.Rifles)
        .Except(Dndcs2.MGs)
        .Except(Dndcs2.Shotguns)
        .ToList();
    
    public Monk(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndClass.Monk,new Collection<DndClassRequirement>())
    {
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new MonkSpawn(),
            new FurryOfBlows(),
        });
    }
    
    public class MonkSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public MonkSpawn() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                constants.DndClass.Monk, null)
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
                if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Monk)
                    return;
                
                var playerStats = PlayerStats.GetPlayerStats(dndPlayer);
                MessagePlayer(player, $"You gained 40% bonus speed for being a {constants.DndClass.Monk}");
                playerStats.ChangeSpeed(.4f);
                
                MessagePlayer(player, $"Furry of Blows: Successive hits increase your damage by 10% (knife hits count as 3)");
            });            
            return HookResult.Continue;
        }
    }
    
    public class FurryOfBlows : EventCallbackFeature<EventPlayerHurt>
    {
        public FurryOfBlows() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Pre, PlayerHurtPre, 
                constants.DndClass.Monk, null)
        {
            
        }

        public static HookResult PlayerHurtPre(EventPlayerHurt @event, GameEventInfo info, DndPlayer dndPlayerVictim,
            DndPlayer dndPlayerAttacker)
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;
            var attackerClassEnum = (constants.DndClass) dndPlayerAttacker.DndClassId;
            
            if(attackerClassEnum != constants.DndClass.Monk )
                return HookResult.Continue;
            
            if (attacker.Team == victim.Team)
                return HookResult.Continue;
            
            var weapon = Dndcs2.GetPlayerWeapon(attacker);
            if(weapon == null)
                return HookResult.Continue;

            var playerStats = PlayerStats.GetPlayerStats(attacker);
            playerStats.FlurryOfBlows++;

            if (weapon.Contains("knife"))
                playerStats.FlurryOfBlows += 2;

            Dndcs2.UpdatePrehookDamage(@event, (int) (@event.DmgHealth * (1 + .1 * playerStats.FlurryOfBlows)));
            return HookResult.Changed;
        }
    }
}