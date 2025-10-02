using System.Collections.ObjectModel;
using CounterStrikeSharp.API.Core;
using static Dndcs2.constants.DndSpecieDescription;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;

namespace Dndcs2.DndSpecies;

public class Human : DndBaseSpecie
{
    public Human(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndSpecie.Human, 0,
            new Collection<DndSpecieRequirement>())
    {        
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new HumanPostPlayerDeathEventCallbackFeature()
        });
        
    }
    

    public class HumanPostPlayerDeathEventCallbackFeature : EventCallbackFeature<EventPlayerDeath>
    {
        public HumanPostPlayerDeathEventCallbackFeature() : 
            base(false, EventCallbackFeaturePriority.Medium, 
                HookMode.Post, PlayerDeathPost, null, constants.DndSpecie.Human)
        {
        }

        public static HookResult PlayerDeathPost(EventPlayerDeath @event, GameEventInfo info, DndPlayer dndPlayer, DndPlayer dndPlayerAttacker)
        {
            var attacker = @event.Attacker;
            if (attacker is null)
                return HookResult.Continue;
            
            var attackerSpecieEnum = (constants.DndSpecie) dndPlayerAttacker.DndSpecieId;
            if (attackerSpecieEnum == constants.DndSpecie.Human && @event.Attacker.Team != @event.Userid.Team)
            {
                DndEvent<EventPlayerDeath>.GrantPlayerExperience(dndPlayerAttacker, Dndcs2.HumanXP.Value, 
                    Dndcs2.HumanXP.Description, nameof(HumanPostPlayerDeathEventCallbackFeature));
            }
            return HookResult.Continue;
        }
    }
}