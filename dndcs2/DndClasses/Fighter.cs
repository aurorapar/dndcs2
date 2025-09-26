using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.DndSpecies;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using DndClass = Dndcs2.dtos.DndClass;

namespace Dndcs2.DndClasses;

public class Fighter : DndBaseClass
{
    public Fighter(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        string dndClassName, string dndClassDescription, Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, (int) constants.DndClass.Fighter, 
            Enum.GetName(typeof(constants.DndClass), constants.DndClass.Fighter), 
            dndClassDescription, dndClassRequirements)
    {
        DndClassSpecieEvents = new List<DndClassSpecieEventFeatureContainer>()
        {
            new FighterExtraPistolDamage(false, DndClassSpecieEventPriority.Medium, 
                HookMode.Pre, FighterExtraPistolDamage.PlayerHurtPre, constants.DndClass.Fighter, null),
        };
    }
    
    public class FighterExtraPistolDamage : DndClassSpecieEventFeature<EventPlayerDeath>
    {
        public FighterExtraPistolDamage(bool overrideDefaultBehavior, DndClassSpecieEventPriority priority, 
            HookMode hookMode, Func<EventPlayerDeath, GameEventInfo, HookResult> callback, constants.DndClass? dndClass = null, 
            constants.DndSpecie? dndSpecie = null) : 
            base(overrideDefaultBehavior, priority, hookMode, callback, dndClass, dndSpecie)
        {
        }

        public static HookResult PlayerHurtPre(EventPlayerDeath @event, GameEventInfo info)
        {
            var attacker = @event.Attacker;
            if (attacker is null)
                return HookResult.Continue;
            
            var dndPlayerAttacker = CommonMethods.RetrievePlayer(attacker);
            var attackerClassEnum = (constants.DndClass) dndPlayerAttacker.DndClassId;
            if (attackerClassEnum == constants.DndClass.Fighter && @event.Attacker.Team != @event.Userid.Team)
            {
                if(new List<string>(){"test"}.Contains(@event.Weapon) )
                {
                    
                }
            }
            return HookResult.Continue;
        }
    }
}