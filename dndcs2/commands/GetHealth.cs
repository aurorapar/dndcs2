using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.commands;

public class GetHealth : DndCommand
{
    public GetHealth() : base("gethealth", "ray traces to a player and prints their health")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        ShowHealth((CCSPlayerController) player);
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        ShowHealth((CCSPlayerController) player);
        return HookResult.Continue;
    }

    public void ShowHealth(CCSPlayerController player)
    {
        var target = Dndcs2.GetViewPlayer(player);
        MessagePlayer(player, $"{target.PlayerName}'s Health {target.PlayerPawn.Value.Health}");
    }
}