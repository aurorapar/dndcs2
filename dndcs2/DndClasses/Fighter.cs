using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Fighter : DndBaseClass
{
    public override PlayerStat GoodStat { get; } = PlayerStat.Strength;
    public override PlayerStat AverageStat { get; } = PlayerStat.Constitution;
    public override PlayerStatRating HealthRating { get; } = PlayerStatRating.High;

    public override List<string> WeaponList { get; } = Dndcs2.Weapons
        .Except(Dndcs2.Snipers)
        .ToList();
    
    public Fighter(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndClass.Fighter,new Collection<DndClassRequirement>())
    {
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new FighterDamageLogic(),
            new AddSnipers(),
        });
    }
    
    public class FighterDamageLogic : EventCallbackFeature<EventPlayerHurt>
    {
        public FighterDamageLogic() : 
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
            var victimClassEnum = (constants.DndClass) dndPlayerVictim.DndClassId;

            if (victimClassEnum == constants.DndClass.Fighter)
            {
                var reduction = @event.DmgHealth * .1;
                reduction = Math.Max(3, reduction);
                reduction = Math.Min(@event.DmgHealth, reduction);
                
                @event.Health += (int) reduction;
                @event.Userid.PlayerPawn.Value.Health += (int) reduction;
                Utilities.SetStateChanged(victim.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
            }
            
            if(attackerClassEnum != constants.DndClass.Fighter )
                return HookResult.Continue;
            
            if (attacker.Team == victim.Team)
                return HookResult.Continue;
            
            var weapon = Dndcs2.GetPlayerWeapon(attacker);
            if(weapon == null)
                return HookResult.Continue;

            if (!Dndcs2.Pistols.Contains(weapon))
                return HookResult.Continue;
            
            Dndcs2.UpdatePrehookDamage(@event, (int) (@event.DmgHealth * 1.25));
            return HookResult.Changed;
        }
    }
    
    public class AddSnipers : EventCallbackFeature<EventPlayerSpawn>
    {
        public AddSnipers() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                constants.DndClass.Fighter, null)
        {
            
        }

        public static HookResult PlayerPostSpawn(EventPlayerSpawn @event, GameEventInfo info, DndPlayer dndPlayer,
            DndPlayer? dndPlayerAttacker)
        {            
            var userid = (int) @event.Userid.UserId;
            Server.NextFrame(() =>
            {
                var player = Utilities.GetPlayerFromUserid(userid);
                dndPlayer = CommonMethods.RetrievePlayer(player);
                var playerBaseStats = PlayerStats.GetPlayerStats(dndPlayer);
                var fighterLevel = CommonMethods.RetrievePlayerClassLevel(player);
                MessagePlayer(player,$"Weapon Proficiency (Pistols): Pistols deal 25% bonus damage");
                MessagePlayer(player,$"Heavy Armor Mastery: Reduces damage by 3 or 10% (larger value)");
                if (fighterLevel > 10)
                {
                    MessagePlayer(player,$"Exotic Weapon Proficiency: Adds the AWP & Auto-Snipers to your repertoire");
                    playerBaseStats.PermitWeapons(Dndcs2.Snipers.ToList());
                }
            });            
            return HookResult.Continue;
        }
    }
}