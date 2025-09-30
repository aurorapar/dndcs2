using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;

namespace Dndcs2.commands;

public class GetPlayerTarget : DndCommand
{
    public GetPlayerTarget() : base("getplayertarget", "ray traces to a player")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        
        Dndcs2.GetViewPlayer(player);  
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        Dndcs2.GetViewPlayer(player); 
        return HookResult.Continue;
    }
}