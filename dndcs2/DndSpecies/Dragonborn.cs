using System.Collections.ObjectModel;
using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;
using Dndcs2.constants;
using Dndcs2.dice;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;
using static Dndcs2.messages.DndMessages;
using DndClass = Dndcs2.constants.DndClass;

namespace Dndcs2.DndSpecies;

public class Dragonborn : DndBaseSpecie
{
    public Dragonborn(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        string dndSpecieName, string dndSpecieDescription, int dndSpecieLevelAdjustment, 
        Collection<DndSpecieRequirement> dndSpecieRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, (int) constants.DndSpecie.Dragonborn, dndSpecieName, 
            dndSpecieLevelAdjustment, dndSpecieDescription, dndSpecieRequirements)
    {        
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new DragonbornBreathWeapon()
        });
        
        Dndcs2.Instance.RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
        {
            if (@event.Userid == null || @event.Userid.ControllingBot)
                return HookResult.Continue;
            
            var userid = (int) @event.Userid.UserId;
            Server.NextFrame(() =>
            {                
                var player = Utilities.GetPlayerFromUserid(userid);
                if (player == null)
                    return;
                var dndPlayer = CommonMethods.RetrievePlayer(player);
                if ((constants.DndSpecie)dndPlayer.DndSpecieId != constants.DndSpecie.Dragonborn)
                    return;                
                
                MessagePlayer(player, $"Fire Resistance: You can walk through molotovs/incindiaries as a {constants.DndSpecie.Dragonborn}");
                MessagePlayer(player, $"Fire Breathing: 10% chance to scorch an enemy with your breathweapon as a {constants.DndSpecie.Dragonborn}");
                

            });
            return HookResult.Continue;
        }, HookMode.Post);
        
    }
    
    public class DragonbornBreathWeapon : EventCallbackFeature<EventPlayerHurt>
    {
        public DragonbornBreathWeapon() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerHurtPost, 
                null, constants.DndSpecie.Dragonborn)
        {
            
        }

        public static HookResult PlayerHurtPost(EventPlayerHurt @event, GameEventInfo info, DndPlayer dndPlayerVictim,
            DndPlayer dndPlayerAttacker)
        {
            var attacker = @event.Attacker;
            var attackerStats = PlayerStats.GetPlayerStats(dndPlayerVictim);
            var victim = @event.Userid;
            var attackerSpecieEnum = (constants.DndSpecie) dndPlayerAttacker.DndSpecieId;
            
            if(attackerSpecieEnum != constants.DndSpecie.Dragonborn)
                return HookResult.Continue;
            
            if (attacker.Team == victim.Team)
                return HookResult.Continue;

            if (new DieRoll(sides: 20, amount: 1).Result >= 20 - (.10 * 20))
            {                
                foreach (var blastTarget in Utilities.GetPlayers())
                {
                    if (CommonMethods.RetrievePlayer(blastTarget).DndSpecieId == (int)constants.DndSpecie.Dragonborn)
                        continue; // Dragonborn should be immune to breathweapons 
                    if (blastTarget.PawnIsAlive && blastTarget.Team == victim.Team &&
                        Vector3.Distance((Vector3)blastTarget.PlayerPawn.Value.AbsOrigin,
                            (Vector3)victim.PlayerPawn.Value.AbsOrigin) <= 300)
                    {
                        var damage = new DieRoll(sides: 6, amount: 2).Result;                        
                        var blastTargetStats = PlayerStats.GetPlayerStats(blastTarget);
                        if(blastTargetStats.MakeDiceCheck(attacker, PlayerStat.Constitution, PlayerStat.Dexterity))
                            damage /= 2;
                        Dndcs2.DamageTarget(attacker, blastTarget, damage);
                        
                        MessagePlayer(attacker,$"Your breath weapon did {damage} damage to {blastTarget.PlayerName}");      
                        Dndcs2.SpawnInfernoGraphic(attacker, blastTarget.PlayerPawn.Value.AbsOrigin);
                    }                        
                }
            }
            
            Dndcs2.UpdatePrehookDamage(@event, (int) (@event.DmgHealth * 1.25));
            return HookResult.Changed;
        }
    }
}