using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using Dndcs2.commands;

namespace Dndcs2;

public partial class Dndcs2
{
    private void RegisterCommands()
    {
        var dndXp = new DndXp();
        var dndInfo = new DndInfo();
        var dndMenu = new DndMenu();
        var playerinfo = new DndPlayerInfo();
        var dndXpLog = new DndXpLog();
        var getplayertarget = new GetPlayerTarget();
        var teleport = new Teleport();
    }
}

public abstract class DndCommand
{
    public string CommandName { get; private set; }
    public string CommandDescription { get; private set; }

    public DndCommand(string commandName, string commandDescription)
    {
        CommandName = commandName;
        CommandDescription = commandDescription;
        
        Dndcs2.Instance.RegisterEventHandler<EventPlayerChat>((@event, info) =>
        {
            if(@event.Text.Equals(CommandName) || @event.Text.StartsWith(CommandName + " "))
                return ChatHandler(@event, info);
            return HookResult.Continue;
        });
        
        Dndcs2.Instance.AddCommand(CommandName, CommandDescription, (player, info) =>
        {
            if (player == null) return;
            CommandHandler(player, info);
        });
        
    }
    
    public abstract void CommandHandler(CCSPlayerController? player, CommandInfo command);
    public abstract HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info);
}