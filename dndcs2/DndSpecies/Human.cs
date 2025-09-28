using System.Collections.ObjectModel;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;
using DndClass = Dndcs2.constants.DndClass;

namespace Dndcs2.DndSpecies;

public class Human : DndBaseSpecie
{
    public Human(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        string dndSpecieName, string dndSpecieDescription, int dndSpecieLevelAdjustment, 
        Collection<DndSpecieRequirement> dndSpecieRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, (int) constants.DndSpecie.Human, dndSpecieName, 
            dndSpecieLevelAdjustment, dndSpecieDescription, dndSpecieRequirements)
    {        
        DndClassSpecieEvents.Add(         
            new HumanPostPlayerDeathEventFeature()
        );
        
    }
    

    public class HumanPostPlayerDeathEventFeature : DndClassSpecieEventFeature<EventPlayerDeath>
    {
        public HumanPostPlayerDeathEventFeature() : 
            base(false, DndClassSpecieEventPriority.Medium, 
                HookMode.Post, PlayerDeathPost, null, constants.DndSpecie.Human)
        {
        }

        public static HookResult PlayerDeathPost(EventPlayerDeath @event, GameEventInfo info, DndPlayer dndPlayer, DndPlayer? notUsed)
        {
            var attacker = @event.Attacker;
            if (attacker is null)
                return HookResult.Continue;
            
            var dndPlayerAttacker = CommonMethods.RetrievePlayer(attacker);
            var attackerSpecieEnum = (constants.DndSpecie) dndPlayerAttacker.DndSpecieId;
            if (attackerSpecieEnum == constants.DndSpecie.Human && @event.Attacker.Team != @event.Userid.Team)
            {
                DndEvent<EventPlayerDeath>.GrantPlayerExperience(dndPlayerAttacker, Dndcs2.HumanXP.Value, 
                    Dndcs2.HumanXP.Description, nameof(HumanPostPlayerDeathEventFeature));
            }
            return HookResult.Continue;
        }
    }
}