using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using static Dndcs2.messages.DndMessages;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.commands;

public class Weapons : DndCommand
{
    public Weapons() : base("!weapons", "Shows weapons someone can use")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        foreach (var log in CommonMethods.RetrieveDndXpLogs(player))
            MessagePlayer(player, log);
        ListWeapons(player);
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        ListWeapons(player);
        return HookResult.Continue;
    }

    public void ListWeapons(CCSPlayerController player)
    {
        var playerStats = PlayerStats.GetPlayerStats(player);
        MessagePlayer(player, "Allowed Weapons for your class & specie:");
        foreach (var item in playerStats.GetAllowedWeapons())
        {
            MessagePlayer(player,"    \t" + item);
        }
    }
}