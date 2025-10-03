using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using Dndcs2.Sql;
using Dndcs2.stats;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public static class ItemPickupHandler
{

    private static Dictionary<Tuple<string, int>, int> _lastPickupMessage = new();
    
    public static HookResult ItemPickup(DynamicHook hook)
    {
        if (hook.GetParam<CCSPlayer_ItemServices>(0).Pawn.Value?.Controller.Value?.As<CCSPlayerController>() is not CCSPlayerController player)
        {
            return HookResult.Continue;
        }
        
        CCSWeaponBaseVData vdata = VirtualFunctions.GetCSWeaponDataFromKeyFunc
                                       .Invoke(-1, hook.GetParam<CEconItemView>(1).ItemDefinitionIndex.ToString())
                                   ?? throw new Exception("Failed to retrieve CCSWeaponBaseVData from ItemDefinitionIndex.");

        var item = vdata.Name.Replace("weapon_", "");
        var playerStats = PlayerStats.GetPlayerStats(player);
        if (!playerStats.CheckWeapon(item))
        {
            var dndPlayer = CommonMethods.RetrievePlayer(player);
            var message =  $"Your class and specie may not use a {item.ToUpper()}! Use !weapons to see available " +
                                  $"{(constants.DndClass)dndPlayer.DndClassId}/{(constants.DndSpecie)dndPlayer.DndSpecieId} weapons";

            var dictKey = new Tuple<string, int>(message, (int)player.UserId);
            
            if (!_lastPickupMessage.ContainsKey(dictKey))
                _lastPickupMessage[dictKey] = (int) (Server.TickCount - Server.TickInterval * 6);
            if ((Server.TickCount - _lastPickupMessage[dictKey]) * Server.TickInterval > 5)
            {
                MessagePlayer(player, message);
                _lastPickupMessage[dictKey] = Server.TickCount;
            }
            return HookResult.Stop;
        }
                
        return HookResult.Continue;
    }
}