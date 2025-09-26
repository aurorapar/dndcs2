using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using Dndcs2.menus;

namespace Dndcs2.commands;

public class DndMenu : DndCommand
{
    public DndMenu() : base("dndmenu", "Shows a menu")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        
        DndMainMenu menu = new();
        menu.Display(player, 10);
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        DndMainMenu menu = new();
        menu.Display(player, 10);
        return HookResult.Continue;
    }
}