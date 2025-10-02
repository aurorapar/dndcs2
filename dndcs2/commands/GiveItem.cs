using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;

namespace Dndcs2.commands;

public class GiveItem : DndCommand
{
    public GiveItem() : base("giveitem", "testing damage stuff")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        
        GiveItemLogic(player);  
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        GiveItemLogic(player); 
        return HookResult.Continue;
    }

    public void GiveItemLogic(CCSPlayerController player)
    {
        player.GiveNamedItem("weapon_molotov");
        
    }
}
