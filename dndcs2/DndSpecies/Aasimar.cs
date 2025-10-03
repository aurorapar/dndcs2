using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
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
            
        });

        Dndcs2.Instance.RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
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
                if ((constants.DndSpecie)dndPlayer.DndSpecieId != constants.DndSpecie.Aasimar)
                    return;
                
                if(dndPlayer.DndClassId != (int) constants.DndClass.Cleric)
                    MessagePlayer(player, $"Blessed: You may use !guidance as an {constants.DndSpecie.Aasimar}");


            });
            return HookResult.Continue;
        }, HookMode.Post);

    }
}