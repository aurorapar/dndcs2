using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using Dndcs2.commands;
using static Dndcs2.commands.SpellsAbilities.DndAbility;

namespace Dndcs2;

public partial class Dndcs2
{
    private void RegisterCommands()
    {
        new DndXp();
        new DndInfo();
        new DndMenu();
        new DndPlayerInfo();
        new DndXpLog();
        new Weapons();
        RegisterAbilities();

        new TestDamage();
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

    public virtual void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        return;
    }

    public virtual HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        return HookResult.Continue;
    }
}