using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using static Dndcs2.messages.DndMessages;
using Dndcs2.Sql;

namespace Dndcs2.commands;

public class DndXpLog : DndCommand
{
    public DndXpLog() : base("dndxplog", "Shows most recent XP events")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        foreach (var log in CommonMethods.RetrieveDndXpLogs(player))
            MessagePlayer(player, log);
        Dndcs2.ShowDndXp(player, player);
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        foreach (var log in CommonMethods.RetrieveDndXpLogs(player))
            MessagePlayer(player, log);
        return HookResult.Continue;
    }
}