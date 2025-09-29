using System.Collections.ObjectModel;
using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Fighter : DndBaseClass
{
    public Fighter(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        string dndClassName, string dndClassDescription, Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, (int) constants.DndClass.Fighter, 
            Enum.GetName(typeof(constants.DndClass), constants.DndClass.Fighter), 
            dndClassDescription, dndClassRequirements)
    {
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new FighterExtraPistolDamage(),
            new FighterExtraHealth()
        });
    }
    
    public class FighterExtraPistolDamage : EventCallbackFeature<EventPlayerHurt>
    {
        public FighterExtraPistolDamage() : 
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
    
    public class FighterExtraHealth : EventCallbackFeature<EventPlayerSpawn>
    {
        public FighterExtraHealth() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                constants.DndClass.Fighter, null)
        {
            
        }

        public static HookResult PlayerPostSpawn(EventPlayerSpawn @event, GameEventInfo info, DndPlayer dndPlayer,
            DndPlayer? dndPlayerAttacker)
        {
            if (@event.Userid == null || @event.Userid.UserId == null)
                return HookResult.Continue;
            if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Fighter)
                return HookResult.Continue;
            
            var playerBaseStats = PlayerStats.GetPlayerStats(dndPlayer);
            var fighterLevel = CommonMethods.RetrievePlayerClassLevel(@event.Userid);
            MessagePlayer(@event.Userid, $"You gained {5 * fighterLevel} bonus health for being a Level {fighterLevel} {constants.DndClass.Fighter}");
            playerBaseStats.ChangeMaxHealth(5 * fighterLevel);
            return HookResult.Continue;
        }
    }
}