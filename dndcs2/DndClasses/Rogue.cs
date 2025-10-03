using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Rogue : DndBaseClass
{
    public Rogue(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndClass.Rogue, 
            PlayerStat.Dexterity, PlayerStat.Intelligence, PlayerStatRating.Average, new Collection<DndClassRequirement>())
    {
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
           
        });
        
        Dndcs2.Instance.RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
        {
            if (@event.Userid == null || @event.Userid.ControllingBot)
                return HookResult.Continue;
            
            var userid = (int) @event.Userid.UserId;
            Server.NextFrame(() =>
            {                
                var player = Utilities.GetPlayerFromUserid(userid);
                if (player == null)
                    return;
                var dndPlayer = CommonMethods.RetrievePlayer(player);
                if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Rogue)
                    return;                
                var playerStats = PlayerStats.GetPlayerStats(dndPlayer);
                MessagePlayer(player, $"You gained 20% bonus speed for being a {constants.DndClass.Rogue}");
                playerStats.ChangeSpeed(.2f);
                
                playerStats.PermitWeapons(
                    Dndcs2.Weapons
                        .Except(Dndcs2.Snipers)
                        .Except(Dndcs2.Rifles)
                        .Except(Dndcs2.MGs)
                        .Except(Dndcs2.Shotguns)
                        .ToList()
                );
            });
            return HookResult.Continue;
        }, HookMode.Post);
    }    
}