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

        var arguments = command.GetCommandString.Split(" ").Skip(1).ToList();
        DoDamage(player, arguments);  
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        var arguments = @event.Text.Split(" ").Skip(1).ToList();
        DoDamage(player, arguments); 
        return HookResult.Continue;
    }

    public void DoDamage(CCSPlayerController player, List<string> args)
    {
        int damage = Int32.Parse(args[0]);
        foreach (var enemy in Utilities.GetPlayers())
        {
            if (enemy.Team == player.Team)
                continue;
            Dndcs2.DamageTarget(enemy, player, damage, damageType: DamageTypes_t.DMG_RADIATION);
            break;
        }
        
    }
}