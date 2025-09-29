using System.Collections.ObjectModel;
using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Rogue : DndBaseClass
{
    public Rogue(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        string dndClassName, string dndClassDescription, Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, (int) constants.DndClass.Rogue, 
            Enum.GetName(typeof(constants.DndClass), constants.DndClass.Rogue), 
            dndClassDescription, dndClassRequirements)
    {
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new RogueSpeed()
        });
    }
    
    public class RogueSpeed : EventCallbackFeature<EventPlayerSpawn>
    {
        public RogueSpeed() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                constants.DndClass.Rogue, null)
        {
            
        }

        public static HookResult PlayerPostSpawn(EventPlayerSpawn @event, GameEventInfo info, DndPlayer dndPlayer,
            DndPlayer? dndPlayerAttacker)
        {
            if (@event.Userid == null || @event.Userid.UserId == null)
                return HookResult.Continue;
            if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Rogue)
                return HookResult.Continue;
            
            var playerBaseStats = PlayerStats.GetPlayerStats(dndPlayer);
            MessagePlayer(@event.Userid, $"You gained 20% bonus speed for being a {constants.DndClass.Rogue}");
            playerBaseStats.ChangeSpeed(.2f);
            return HookResult.Continue;
        }
    }
}