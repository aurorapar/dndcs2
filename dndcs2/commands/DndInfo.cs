using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;

namespace Dndcs2.commands;

public class DndInfo : DndCommand
{
    public DndInfo() : base("dndinfo", "shows info about the mod")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        
        Dndcs2.ShowDndInfo(player);  
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        Dndcs2.ShowDndInfo(player);
        return HookResult.Continue;
    }
}