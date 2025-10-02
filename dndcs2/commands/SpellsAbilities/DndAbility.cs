using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using static Dndcs2.messages.DndMessages;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.commands.SpellsAbilities;

public abstract class DndAbility : DndCommand
{
    private List<AbilityClassSpecieRequirement> ClassSpecieRequirements = new();
    
    private int ManaCost = 0;
    private int? LimitedUses;
    private double AbilityCooldown = .1;  
    
    public DndAbility(List<AbilityClassSpecieRequirement> requirements, int manaCost, int? limitedUses, double abilityCooldown, 
        string commandName, string commandDescription) : 
        base(commandName, commandDescription)
    {
        ClassSpecieRequirements = requirements;
        ManaCost = manaCost;
        LimitedUses = limitedUses;
        AbilityCooldown = abilityCooldown;
        
        Dndcs2.Instance.Log.LogInformation("Registered ability " + GetType().Name + " as " + CommandName);
    }
    
    public static void RegisterAbilities()
    {
        new Guidance();
        new Mana();
        new ColorSpray();
    }
    
    public sealed override void CommandHandler(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;

        var arguments = command.GetCommandString.Replace(CommandName + " ", " ").Split(" ").ToList();
        VerifyCastChecks(player, arguments);
    }

    public sealed override HookResult ChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if(player == null)
            return HookResult.Continue;
        
        var arguments = @event.Text.Replace(CommandName + " ", " ").Split(" ").ToList();
        VerifyCastChecks(player, arguments);
        return HookResult.Continue;
    }

    private void VerifyCastChecks(CCSPlayerController player, List<string> arguments)
    {
        if (!player.PawnIsAlive)
            return;

        if (!CheckClassSpecieRequirements(player))
        {
            Dndcs2.Instance.Log.LogInformation($"Player failed to meet class/specie requirements for {CommandName}");
            return;
        }
        
        var playerStats = PlayerStats.GetPlayerStats(player);
        if (playerStats.Mana < ManaCost)
        {
            MessagePlayer(player, "You do not have the necessary mana to cast this spell!");
            return;
        }
        
        var cooldown = playerStats.AbilityCooldown;
        var currentTick = Server.TickCount;
        if ((currentTick - cooldown) * Server.TickInterval < AbilityCooldown)
        {
            MessagePlayer(player, "You are on cooldown still!");
            return;
        }

        if (LimitedUses != null)
        {
            if(!playerStats.SpellLimitedUses.ContainsKey(CommandName))
                playerStats.SpellLimitedUses.Add(CommandName, (int) LimitedUses);
            if(playerStats.SpellLimitedUses[CommandName] <= 0)
                MessagePlayer(player, "You have run out of uses for this spell!");
        }

        if (UseAbility(player, playerStats, arguments))
        {
            playerStats.ChangeMana(ManaCost * -1);
            if(playerStats.SpellLimitedUses.ContainsKey(CommandName))
                 playerStats.SpellLimitedUses[CommandName]--;            
        }
    }

    private bool CheckClassSpecieRequirements(CCSPlayerController player)
    {
        var dndPlayer = CommonMethods.RetrievePlayer(player);
        var playerClass = (constants.DndClass)dndPlayer.DndClassId;
        var playerSpecie = (constants.DndSpecie)dndPlayer.DndSpecieId;
        foreach (var req in ClassSpecieRequirements)
        {
            if (req.DndClass != null && req.DndSpecie == null)
            {
                Dndcs2.Instance.Log.LogInformation("Checking class requirements");
                if (req.DndClass != playerClass)
                    if (CommonMethods.RetrievePlayerClassLevel(player) < (req.ClassLevel ?? 1))
                        continue;
                
                return true;
            }
            
            if (req.DndSpecie != null && req.DndClass == null)
            {
                Dndcs2.Instance.Log.LogInformation("Checking specie requirements");
                if (req.DndSpecie != playerSpecie)
                    if (CommonMethods.RetrievePlayerSpecieLevel(player) < (req.SpecieLevel ?? 1))
                        continue;
                
                return true;
            }

            if (req.DndSpecie != null && req.DndClass != null)
            {
                if(playerClass != req.DndClass && playerSpecie != req.DndSpecie)                    
                    if(CommonMethods.RetrievePlayerClassLevel(player) < (req.ClassLevel ?? 1))
                        if (CommonMethods.RetrievePlayerSpecieLevel(player) < (req.SpecieLevel ?? 1))
                            continue;
                
                return true;
            }
        }

        return false;
    }
    
    public abstract bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments);    
}
