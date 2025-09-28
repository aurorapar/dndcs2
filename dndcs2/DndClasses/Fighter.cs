using System.Collections.ObjectModel;
using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;

namespace Dndcs2.DndClasses;

public class Fighter : DndBaseClass
{
    public Fighter(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        string dndClassName, string dndClassDescription, Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, (int) constants.DndClass.Fighter, 
            Enum.GetName(typeof(constants.DndClass), constants.DndClass.Fighter), 
            dndClassDescription, dndClassRequirements)
    {
        DndClassSpecieEvents.Add(        
            new FighterExtraPistolDamage()            
        );
    }
    
    public class FighterExtraPistolDamage : DndClassSpecieEventFeature<EventPlayerHurt>
    {
        public FighterExtraPistolDamage() : 
            base(false, DndClassSpecieEventPriority.Medium, HookMode.Pre, PlayerHurtPre, 
                constants.DndClass.Fighter, null)
        {
            PrintMessageToConsole("Creating " + GetType().Name);
        }

        public static HookResult PlayerHurtPre(EventPlayerHurt @event, GameEventInfo info, DndPlayer dndPlayerVictim,
            DndPlayer dndPlayerAttacker)
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;
            var attackerClassEnum = (constants.DndClass) dndPlayerAttacker.DndClassId;
            if (attackerClassEnum == constants.DndClass.Fighter && attacker.Team != victim.Team)
            {
                var weapon = Dndcs2.GetPlayerWeapon(attacker);
                if(weapon == null)
                    return HookResult.Continue;

                if (Dndcs2.Pistols.Contains(weapon))
                {
                    Dndcs2.UpdatePrehookDamage(@event, (int) (@event.DmgHealth * 1.25));
                    return HookResult.Changed;
                }
            }
            return HookResult.Continue;
        }
    }
}