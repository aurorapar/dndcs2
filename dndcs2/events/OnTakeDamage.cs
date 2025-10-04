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

        CCSPlayerController? victim = null;
        if (victimEntity.DesignerName == "player")
            victim = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerPawn.Value.Handle == victimEntity.Handle);
        
        if(victim != null)
        {
            var dndVictim = CommonMethods.RetrievePlayer(victim);
            var victimSpecie = (constants.DndSpecie)dndVictim.DndSpecieId;

            if (damageInfo.Inflictor.Value.DesignerName == "inferno" && victimSpecie == constants.DndSpecie.Dragonborn)
                return HookResult.Stop;
        }

        if (damageInfo.Attacker.Value == null || !damageInfo.Attacker.IsValid || damageInfo.Attacker.Value.DesignerName != "player")
            return HookResult.Continue;
        var attackerPawn = damageInfo.Attacker.Value.As<CCSPlayerPawn>();
        var attacker = Utilities.GetPlayers().First(p => p.PlayerPawn.Value.Handle ==  attackerPawn.Handle);
        
        var dndAttacker = CommonMethods.RetrievePlayer(attacker);
        if (dndAttacker.DndClassId == (int)constants.DndClass.Monk)
        {
            var monkStats = PlayerStats.GetPlayerStats(attacker);
            if (victim != null && victim.Team == attacker.Team)
            {
                monkStats.FlurryOfBlows = 0;
                MessagePlayer(attacker, "You attacked a teammate and lost your Flurry stacks");
            }
            else
            {
                var tickCount = Server.TickCount;
                int attackerUserid = (int)attacker.UserId;

                if (!monkStats.MonkHits.ContainsKey(tickCount))
                    monkStats.MonkHits[tickCount] = new List<string>();
                monkStats.MonkHits[tickCount].Add(victimEntity.DesignerName);

                Server.NextFrame(() =>
                {
                    var futureMonkStats = PlayerStats.GetPlayerStats(attackerUserid);
                    if (futureMonkStats.MonkHits[tickCount].Any(v => v == "player"))
                        return;
                    if (!futureMonkStats.FlurryMessages.Contains(tickCount))
                    {
                        var futureAttacker = Utilities.GetPlayerFromUserid(attackerUserid);
                        MessagePlayer(futureAttacker, "You missed an attack and lost your Flurry stacks");
                        futureMonkStats.FlurryOfBlows = 0;
                        futureMonkStats.FlurryMessages.Add(tickCount);
                    }
                });
            }
        }

        
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
                MessagePlayer(druid, $"One of your Good Chickens healed {attacker.PlayerName}");
                PlayerStats.GetPlayerStats(attacker).ChangeHealth(10);
                break;
            }
        }

        return HookResult.Continue;
    }
}
