using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using Dndcs2.DndClasses;
using static Dndcs2.messages.DndMessages;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.events;

public static class OnTakeDamageHook 
{
    public static HookResult OnTakeDamage(DynamicHook hook)
    {
        var victimEntity = hook.GetParam<CEntityInstance>(0);
        var damageInfo = hook.GetParam<CTakeDamageInfo>(1);
        
        var player = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerPawn.Value.Handle == victimEntity.Handle);
        if (player != null)
        {
            var dndVictim = CommonMethods.RetrievePlayer(player);
            var victimSpecie = (constants.DndSpecie)dndVictim.DndSpecieId;

            if (damageInfo.Inflictor.Value.DesignerName == "inferno" && victimSpecie == constants.DndSpecie.Dragonborn)
            {
                MessagePlayer(player, "Blocked damage");

                return HookResult.Stop;
            }
        }

        
        var attacker = Utilities.GetPlayers()
            .FirstOrDefault(p => p.PlayerPawn.Value.Index == damageInfo.Attacker.Index);
        Dndcs2.Instance.Log.LogInformation(attacker.PlayerName);
        var druids = Utilities.GetPlayers().Where(p => 
            PlayerStats.GetPlayerStats(p).Chickens.Any()
            && p.TeamNum == attacker.TeamNum
        );
        
        foreach (var druid in druids)
        {
            var chickens = PlayerStats.GetPlayerStats(druid).Chickens;
            if (chickens.Select(c => c.Index).Contains(victimEntity.Index))
            {

                MessagePlayer(attacker, "A Druid's Good Chicken healed you for 10HP!");
                PlayerStats.GetPlayerStats(attacker).ChangeHealth(10);
                break;
            }
        }

        return HookResult.Continue;
    }
}
