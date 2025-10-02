using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;
using Dndcs2.Sql;
using Dndcs2.stats;
using Dndcs2.timers;
using static Dndcs2.messages.DndMessages;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

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
        
        SpawnSomething(player);
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        SpawnSomething(player);
        return HookResult.Continue;
    }

    public static void SpawnSomething(CCSPlayerController player)
    {
        var playerStats = PlayerStats.GetPlayerStats(player);
        var locationV3 = Dndcs2.GetViewLocation(player);
        var location = new Vector(locationV3.X, locationV3.Y, locationV3.Z);
        playerStats.InfernoLocation = location;
        if (location == null)
         return;
        //var location = new Vector(target.PlayerPawn.Value.AbsOrigin.X, target.PlayerPawn.Value.AbsOrigin.Y, target.PlayerPawn.Value.AbsOrigin.Z);

        var grenade = Dndcs2.SpawnMolotovGrenade(location, new QAngle(0,0,0), new Vector(0,0,0), player.Team);
        grenade.DetonateTime = 0;
        grenade.Thrower.Raw = player.PlayerPawn.Raw;

        Server.NextFrame( () =>
        {
            var infernoLocations = Utilities.GetPlayers()
                .Where(p => 
                    CommonMethods.RetrievePlayer(p).DndSpecieId == (int)constants.DndSpecie.Dragonborn 
                    && PlayerStats.GetPlayerStats(p).InfernoLocation != null)
                .Select(p => PlayerStats.GetPlayerStats(p).InfernoLocation).ToList();
            
            var infernos = Utilities.GetAllEntities()
             .Where(e => e.DesignerName == "inferno");
            if (!infernos.Any())
            {
                BroadcastMessage("No infernos found");
                return;
            }
            foreach (var i in infernos)
            {

                var inferno = Utilities.GetEntityFromIndex<CInferno>((int)i.Index);
                if (inferno == null)
                {
                     BroadcastMessage("No inferno found");
                     continue;
                }                     
                
                infernoLocations.ForEach(l =>
                {
                    if (!(Vector3.Distance((Vector3)inferno.AbsOrigin, (Vector3)location) > 1))
                        return;                
                    BroadcastMessage("Found the inferno");
                    Server.NextFrame(() => { inferno.Remove(); });    
                });                            
            }
            BroadcastMessage("Done processing next frame");
        });

         // var entity = Utilities.CreateEntityByName<CInferno>("inferno");
        // if (entity == null)
        // {
        //     PrintMessageToConsole($"Failed to create entity!");
        //     return;
        // }
        //
        // string playerName = player.PlayerName;
        //
        // var pos = Dndcs2.GetViewLocation(player);
        // pos.Z += 20;
        // PrintMessageToConsole($"Trying to create inferno at {pos}");
        // entity.Teleport(pos);
        // entity.AbsOrigin.X = pos.X;
        // entity.AbsOrigin.Y = pos.Y;
        // entity.AbsOrigin.Z = pos.Z;
        // entity.OriginalSpawnLocation.X = pos.X;
        // entity.OriginalSpawnLocation.Y = pos.Y;
        // entity.OriginalSpawnLocation.Z = pos.Z;
        // entity.FireCount = 1;
        // entity.InfernoType = 0;
        // entity.MaxFlames = 16;
        // entity.SpreadCount = 0;
        // entity.FireSpawnOffset = 0;
        // entity.FireEffectTickBegin = Server.TickCount;
        // entity.FireLifetime = 2000;
        // entity.Globalname = $"{playerName}_inferno";
        // entity.TeamNum = player.TeamNum;
        // entity.ActiveTimer
        // entity.DispatchSpawn();
    }
    
}