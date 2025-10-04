using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.DndSpecies;

public class Aasimar : DndBaseSpecie
{
    public Aasimar(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndSpecie.Aasimar, 0,
            new Collection<DndSpecieRequirement>())
    {
        DndClassSpecieEvents.AddRange(new List<EventCallbackFeatureContainer>()
        {
            new AasimarSpawn()
        });
    }
    
    public class AasimarSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public AasimarSpawn() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                null, constants.DndSpecie.Aasimar)
        {
            
        }

        public static HookResult PlayerPostSpawn(EventPlayerSpawn @event, GameEventInfo info, DndPlayer dndPlayer,
            DndPlayer? dndPlayerAttacker)
        {
            var userid = (int)@event.Userid.UserId;
            Server.NextFrame(() =>
            {
                var player = Utilities.GetPlayerFromUserid(userid);
                if (player == null)
                    return;
                var dndPlayer = CommonMethods.RetrievePlayer(player);
                if ((constants.DndSpecie)dndPlayer.DndSpecieId != constants.DndSpecie.Aasimar)
                    return;
                
                if(dndPlayer.DndClassId != (int) constants.DndClass.Cleric)
                    MessagePlayer(player, $"Blessed: You may use !bless as an {constants.DndSpecie.Aasimar}");
            });
            return HookResult.Continue;
        }
    }
}