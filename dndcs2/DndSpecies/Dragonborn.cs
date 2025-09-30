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
        
    }
    
    public class DragonbornBreathWeapon : EventCallbackFeature<EventPlayerHurt>
    {
        public DragonbornBreathWeapon() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Pre, PlayerHurtPre, 
                null, constants.DndSpecie.Dragonborn)
        {
            
        }

        public static HookResult PlayerHurtPre(EventPlayerHurt @event, GameEventInfo info, DndPlayer dndPlayerVictim,
            DndPlayer dndPlayerAttacker)
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;
            var attackerSpecieEnum = (constants.DndSpecie) dndPlayerAttacker.DndSpecieId;
            
            if(attackerSpecieEnum != constants.DndSpecie.Dragonborn)
                return HookResult.Continue;
            
            if (attacker.Team == victim.Team)
                return HookResult.Continue;

            if (new DieRoll(sides: 20, amount: 1).Result >= 1)
            {
                var location = victim.PlayerPawn.Value.AbsOrigin;
                var newLocation = Dndcs2.PlaceGrenade((Vector3)location);
                location = new Vector(newLocation.X, newLocation.Y, newLocation.Z + 125);
                var grenade = Dndcs2.SpawnMolotovGrenade(location, new QAngle(0,0,0), new Vector(0,0,0), attacker.Team);
                grenade.DetonateTime = 0;
                grenade.Thrower.Raw = attacker.PlayerPawn.Raw;
                
                foreach (var blastTarget in Utilities.GetPlayers())
                {
                    if (blastTarget.PawnIsAlive && blastTarget.Team == victim.Team &&
                        Vector3.Distance((Vector3)blastTarget.PlayerPawn.Value.AbsOrigin,
                            (Vector3)victim.PlayerPawn.Value.AbsOrigin) <= 300)
                    {
                        var damage = new DieRoll(sides: 6, amount: 2).Result;
                        Dndcs2.DamageTarget(attacker, blastTarget, damage);
                        MessagePlayer(attacker,$"Your breath weapon did {damage} to {blastTarget.PlayerName}");                        
                    }                        
                }
            }
            
            Dndcs2.UpdatePrehookDamage(@event, (int) (@event.DmgHealth * 1.25));
            return HookResult.Changed;
        }
    }
}