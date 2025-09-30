using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;

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
        var target = player;
        foreach (var candidate in Utilities.GetPlayers())
        {
            if (candidate.PawnIsAlive && candidate.Team != player.Team)
            {
                target = candidate;
                break;
            }
        }
        var viewLocation = Dndcs2.GetViewLocation(player, 700, 100);
        //viewLocation.Z += player.PlayerPawn.Value.ViewOffset.Z;
        target.PlayerPawn.Value.Teleport(viewLocation);
        int userId = (int) target.UserId;
        Server.NextFrame(() =>
        {
            var teleportingPlayer = Utilities.GetPlayerFromUserid(userId);
            Dndcs2.IsPlayerStuck(teleportingPlayer);    
        });        
    }
}