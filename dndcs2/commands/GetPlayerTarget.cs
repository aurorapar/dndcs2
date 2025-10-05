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

        Logic(player);
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;

        Logic(player);
        
        return HookResult.Continue;
    }

    public static void Logic(CCSPlayerController player)
    {
        var rogueAngle = player.PlayerPawn.Value.EyeAngles;
        var angle = rogueAngle.Y;
        if (angle < 0)
            angle += 360;
        Dndcs2.Instance.Log.LogInformation($"{angle}");
    }
}