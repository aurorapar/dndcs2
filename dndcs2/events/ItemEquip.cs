using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using Dndcs2.Sql;
using Dndcs2.stats;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class ItemEquip : DndEvent<EventItemEquip>
{

    private static Dictionary<Tuple<string, int>, int> _lastPickupMessage = new();
    
    public ItemEquip() : base()
    {
        
    }
    
    public override HookResult DefaultPreHookCallback(EventItemEquip @event, GameEventInfo info)
    {
        if(@event.Userid == null || @event.Userid.TeamNum == 0)
            return HookResult.Continue;

        var player = @event.Userid;
        var item = @event.Item.Replace("weapon_", "");
        var playerStats = PlayerStats.GetPlayerStats(player);
        if (!playerStats.CheckWeapon(item))
        {
            var dndPlayer = CommonMethods.RetrievePlayer(player);
            
            string item_display = item;
            if (item_display.StartsWith("knife_"))
                item_display = "knife";
            item_display = item_display.Replace("item_", "").Replace("weapon_", "");
            
            var message =  $"Your class and specie may not use a {item_display}! Use !weapons to see available " +
                                  $"{(constants.DndClass)dndPlayer.DndClassId}/{(constants.DndSpecie)dndPlayer.DndSpecieId} weapons";

            var dictKey = new Tuple<string, int>(message, (int)player.UserId);

            if (!_lastPickupMessage.ContainsKey(dictKey))
                _lastPickupMessage[dictKey] = (int)(Server.TickCount - Server.TickInterval * 6);

            if ((Server.TickCount - _lastPickupMessage[dictKey]) * Server.TickInterval > 5)
            {
                MessagePlayer(player, message);
                _lastPickupMessage[dictKey] = Server.TickCount;
            }
            player.DropActiveWeapon();
            return HookResult.Stop;
        }
                
        return HookResult.Continue;
    }
}