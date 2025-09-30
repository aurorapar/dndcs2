using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Dndcs2.timers;
using Timer = System.Threading.Timer;

namespace Dndcs2.commands;

public class Flashbang : DndCommand
{
    public Flashbang() : base("flashbang", "spawns a flashbang")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        
        SpawnFlashbang(player);
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        SpawnFlashbang(player);
        return HookResult.Continue;
    }

    public static void SpawnFlashbang(CCSPlayerController player)
    {
        
        var location = Dndcs2.GetViewLocation(player, 300, 50);
        location.Z += 80;
        player.PrintToChat($"First Location {location}");
        location = Dndcs2.PlaceGrenade(location);
        player.PrintToChat($"Second Location {location}");
        var grenade = Dndcs2.SpawnMolotovGrenade(new Vector(location.X, location.Y, location.Z), new QAngle(0,0,0), new Vector(0,0,0), player.Team);
        grenade.DetonateTime = 0;
        grenade.Thrower.Raw = player.PlayerPawn.Raw;
    }
    
}