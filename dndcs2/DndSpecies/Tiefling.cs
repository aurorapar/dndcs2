using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.DndSpecies;

public class Tiefling : DndBaseSpecie
{
    public Tiefling(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndSpecie.Tiefling, 0,
            new Collection<DndSpecieRequirement>())
    {
        DndClassSpecieEvents.AddRange(new List<EventCallbackFeatureContainer>()
        {
            new TieflingSpawn()
        });
    }
    
    public class TieflingSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public TieflingSpawn() : 
            base(false, EventCallbackFeaturePriority.Medium, 
                HookMode.Post, SpawnPost, null, constants.DndSpecie.Human)
        {
            
        }

        public static HookResult SpawnPost(EventPlayerSpawn @event, GameEventInfo info, DndPlayer dndPlayer, DndPlayer? dndPlayerAttacker)
        {
            if (@event.Userid == null || @event.Userid.ControllingBot)
                return HookResult.Continue;

            var userid = (int)@event.Userid.UserId;
            Server.NextFrame(() =>
            {
                var player = Utilities.GetPlayerFromUserid(userid);
                if (player == null)
                    return;
                var dndPlayer = CommonMethods.RetrievePlayer(player);
                if ((constants.DndSpecie)dndPlayer.DndSpecieId != constants.DndSpecie.Tiefling)
                    return;
                
                if(dndPlayer.DndClassId != (int) constants.DndClass.Cleric)
                    MessagePlayer(player, $"Cursed: You may use !bane as an {constants.DndSpecie.Tiefling}");
            });
            return HookResult.Continue;
        }
    }
}