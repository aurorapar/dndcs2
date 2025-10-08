using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using Dndcs2.dtos;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.commands;

public class CheatXP : DndCommand
{
    public CheatXP() : base("cheatxp", "testing damage stuff")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;

        var arguments = command.GetCommandString.Split(" ").Skip(1).ToList();
        DoLogic(player, arguments);  
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        var arguments = @event.Text.Split(" ").Skip(1).ToList();
        DoLogic(player, arguments); 
        return HookResult.Continue;
    }

    public void DoLogic(CCSPlayerController player, List<string> args)
    {
        int xpAmount = Int32.Parse(args[0]);
        var xp = new DndExperienceLog(
            "cheat " + player.PlayerName, 
            DateTime.UtcNow, 
            "cheat " + player.PlayerName, 
            DateTime.UtcNow, 
            true, 
            CommonMethods.RetrievePlayer(player).DndPlayerId, 
            xpAmount,
            "cheat"
        );
        CommonMethods.GrantExperience(player, xp);
    }
}