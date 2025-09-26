using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;

namespace Dndcs2.commands;

public class DndXp : DndCommand
{
    public DndXp() : base("dndxp", "Shows player their XP")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;

        Dndcs2.ShowDndXp(player, player);
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        Dndcs2.ShowDndXp(player, player);
        return HookResult.Continue;
    }
}