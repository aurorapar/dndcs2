using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using static Dndcs2.messages.DndMessages;
using Dndcs2.Sql;

namespace Dndcs2.events;

public static class OnTakeDamageHook 
{
    public static HookResult OnTakeDamage(DynamicHook hook)
    {
        var victimEntity = hook.GetParam<CEntityInstance>(0);
        var player = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerPawn.Value.Handle == victimEntity.Handle);
        if (player == null)        
            return HookResult.Continue;
        
        
        var damageInfo = hook.GetParam<CTakeDamageInfo>(1);        
        var dndVictim = CommonMethods.RetrievePlayer(player);
        var victimSpecie = (constants.DndSpecie) dndVictim.DndSpecieId;

        if (damageInfo.Inflictor.Value.DesignerName == "inferno" && victimSpecie == constants.DndSpecie.Dragonborn)
        {
            MessagePlayer(player, "Blocked damage");
            
            return HookResult.Stop;
        }
            

        return HookResult.Continue;
    }
}
