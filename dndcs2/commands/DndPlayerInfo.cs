using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.commands;

public class DndPlayerInfo : DndCommand
{
    public DndPlayerInfo() : base("playerinfo", "Shows player someone else's stats")
    {
        
    }

    public override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        var target = GetTarget(command.GetCommandString);
        if (target == null)
        {
            MessagePlayer(player, "Could not find a player with that name");
            return;
        }

        Dndcs2.ShowDndXp(player, target);
    }

    public override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        
        var target = GetTarget(@event.Text);
        if (target == null)
        {
            MessagePlayer(player, "Could not find a player with that name");
            return HookResult.Continue;
        }
        Dndcs2.ShowDndXp(player, target);
        
        return HookResult.Continue;
    }

    public CCSPlayerController? GetTarget(string inputCommand)
    {
        inputCommand = inputCommand.Replace(CommandName + " ", "");
        foreach(var player in Utilities.GetPlayers())
            if (player.PlayerName.ToLower().Contains(inputCommand.ToLower()))
                return player;

        return null;
    }
}