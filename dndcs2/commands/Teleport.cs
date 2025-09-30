using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.commands;

public class Teleport : DndCommand
{
    public Teleport() : base("teleport", "ray traces to a player")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        DoTeleport((CCSPlayerController) player);
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        DoTeleport((CCSPlayerController) player);
        return HookResult.Continue;
    }

    public void DoTeleport(CCSPlayerController player)
    {        
        var viewLocation = Dndcs2.GetViewLocation(player, 700, 100);
        //viewLocation.Z += player.PlayerPawn.Value.ViewOffset.Z;
        player.PlayerPawn.Value.Teleport(viewLocation);
        int userId = (int) player.UserId;
        Server.NextFrame(() =>
        {
            var teleportingPlayer = Utilities.GetPlayerFromUserid(userId);
            Dndcs2.IsPlayerStuck(teleportingPlayer);    
        });        
    }
}