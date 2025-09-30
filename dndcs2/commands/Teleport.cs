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
        Vector eyePosition = (Vector) player.PlayerPawn.Value.AbsOrigin;
        var startPosition = (eyePosition.X, eyePosition.Y, eyePosition.Z);
        startPosition.Z += player.PlayerPawn.Value.ViewOffset.Z;

        QAngle eyeAngles = player.PlayerPawn.Value.EyeAngles;
            
        Vector forward = new();
            
        NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, 0, 0);
        Vector endOrigin = new(startPosition.X + forward.X * 50, startPosition.Y + forward.Y * 50, startPosition.Z + forward.Z * 50);
        
        foreach(var target in Utilities.GetPlayers())
        {
            if (target.PawnIsAlive && target.Team != player.Team)
            {
                MessagePlayer(player, $"Teleporting {target.PlayerName}");
                target.PlayerPawn.Value.Teleport(endOrigin);
                break;
            }
        }
    }
}