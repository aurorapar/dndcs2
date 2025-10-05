using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;

namespace Dndcs2.commands;

public class TestDamage : DndCommand
{
    public TestDamage() : base("testdamage", "testing damage stuff")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        
        DoDamage(player);  
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        DoDamage(player); 
        return HookResult.Continue;
    }

    public void DoDamage(CCSPlayerController player)
    {
        Dndcs2.DamageTarget(player, player, 50, damageType: DamageTypes_t.DMG_RADIATION);
    }
}