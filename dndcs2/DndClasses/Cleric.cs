using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;
using Dndcs2.dtos;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Cleric : DndBaseClass
{
    public Cleric(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndClass.Cleric,
            PlayerStat.Wisdom, PlayerStat.Charisma, PlayerStatRating.Average, new Collection<DndClassRequirement>())
    {
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
                if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Cleric)
                    return;

                var playerStats = PlayerStats.GetPlayerStats(dndPlayer);
                var clericLevel = CommonMethods.RetrievePlayerClassLevel(player);

                int mana = (clericLevel + 1) / 2 * 5 + 10;
                mana += clericLevel / 2 * 10;
                playerStats.ChangeMana(mana);
                playerStats.ChangeMaxMana(mana);
                
                MessagePlayer(player,
                    $"You have {playerStats.Mana} mana for being a Level {clericLevel} {constants.DndClass.Cleric}");
                

            });
            return HookResult.Continue;
        }, HookMode.Post);
    }    
}