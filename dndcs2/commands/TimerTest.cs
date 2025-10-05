using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using Dndcs2.timers;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.commands;

public class TimerTest : DndCommand
{
    public TimerTest() : base("timertest", "testing timer")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        
        DoTest(player);  
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        DoTest(player); 
        return HookResult.Continue;
    }

    public void DoTest(CCSPlayerController player)
    {
        new GenericTimer(1, 1, 1, () => BroadcastMessage(Guid.NewGuid().ToString()));
    }
}