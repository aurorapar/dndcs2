using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.dice;
using static Dndcs2.messages.DndMessages;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Rogue : DndBaseClass
{
    public override PlayerAbility GoodStat { get; } = PlayerAbility.Dexterity;
    public override PlayerAbility AverageStat { get; } = PlayerAbility.Intelligence;
    public override PlayerStatRating HealthRating { get; }= PlayerStatRating.Average;

    public override List<string> WeaponList { get; } = Dndcs2.Weapons
        .Except(Dndcs2.Snipers)
        .Except(Dndcs2.Rifles)
        .Except(Dndcs2.MGs)
        .Except(Dndcs2.Shotguns)
        .ToList();
    
    public Rogue(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndClass.Rogue,new Collection<DndClassRequirement>())
    {
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new RogueSpawn(),
            new SneakAttack(),
        });
    }    
    
    public class RogueSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public RogueSpawn() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                constants.DndClass.Rogue, null)
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
                if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Rogue)
                    return;                
                var playerStats = PlayerStats.GetPlayerStats(dndPlayer);
                var rogueLevel = CommonMethods.RetrievePlayerClassLevel(player);
                MessagePlayer(player, $"Dash: You gained 20% bonus speed for being a {constants.DndClass.Rogue}");
                playerStats.ChangeSpeed(.2f);
                MessagePlayer(player, $"Sneak Attack: Attacking enemies from behind deals {Math.Max(1, rogueLevel/2)}d6 bonus damage");
            });            
            return HookResult.Continue;
        }
    }
    
    public class SneakAttack : EventCallbackFeature<EventPlayerHurt>
    {
        public SneakAttack() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Pre, PlayerHurtPre, 
                constants.DndClass.Rogue, null)
        {
            
        }

        public static HookResult PlayerHurtPre(EventPlayerHurt @event, GameEventInfo info, DndPlayer dndPlayerVictim,
            DndPlayer dndPlayerAttacker)
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;
            var attackerClassEnum = (constants.DndClass) dndPlayerAttacker.DndClassId;
            
            if(attackerClassEnum != constants.DndClass.Rogue )
                return HookResult.Continue;
            
            if (attacker.Team == victim.Team)
                return HookResult.Continue;
            
            var weapon = Dndcs2.GetPlayerWeapon(attacker);
            if(weapon == null)
                return HookResult.Continue;

            var rogueLevel = CommonMethods.RetrievePlayerClassLevel(attacker);

            var rogueAngle = attacker.PlayerPawn.Value.EyeAngles;
            var victimAngle = victim.PlayerPawn.Value.EyeAngles;

            var diff = rogueAngle.Y - victimAngle.Y;
            diff = Math.Abs((diff + 180) % 360 - 180);
            if (diff <= 90)
            {
                var dieRoll = new DieRoll(sides: 6, amount: Math.Max(1, rogueLevel / 2));
                MessagePlayer(attacker, $"Your sneak attack did {dieRoll.Result} bonus damage!");
                Dndcs2.UpdatePrehookDamage(@event, (int) (@event.DmgHealth + dieRoll.Result));
            }
            
            return HookResult.Changed;
        }
    }
}