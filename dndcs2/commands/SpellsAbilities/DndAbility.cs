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
    public int? SpecieLimitedUses { get; private set; }
    public static Dictionary<string, DndAbility> DndAbilities = new();
    
    public DndAbility(List<AbilityClassSpecieRequirement> requirements, int manaCost, int? limitedUses, double abilityCooldown, 
        int? specieLimitedUses, string commandName, string commandDescription) : 
        base(commandName, commandDescription)
    {
        ClassSpecieRequirements = requirements;
        ManaCost = manaCost;
        LimitedUses = limitedUses;
        AbilityCooldown = abilityCooldown;
        SpecieLimitedUses = specieLimitedUses;
        
        Dndcs2.Instance.Log.LogInformation("Registered ability " + GetType().Name + " as " + CommandName);
    }
    
    public static void RegisterAbilities()
    {
        var spells = new List<DndAbility>()
        {
            new Guidance(),
            new Mana(),
            new ColorSpray(),
        };
        foreach(var spell in spells)
            DndAbilities[spell.CommandName] = spell;
    }

    public List<AbilityClassSpecieRequirement> GetClassSpecieRequirements()
    {
        return new List<AbilityClassSpecieRequirement>(ClassSpecieRequirements);
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

        bool castingWithSpecie = IsCastingWithSpecie(playerStats, player);
        if (castingWithSpecie)
        {
            if (SpecieLimitedUses < 1)
            {
                MessagePlayer(player, $"You have no more uses of {CommandName}");
                return;
            }                
        }
        else
        {
            if (playerStats.Mana < ManaCost)
            {
                MessagePlayer(player, "You do not have the necessary mana to cast this spell!");
                return;
            }
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
            if (castingWithSpecie)
                SpecieLimitedUses--;
            else
            {
                playerStats.ChangeMana(ManaCost * -1);
                if (playerStats.SpellLimitedUses.ContainsKey(CommandName))
                    playerStats.SpellLimitedUses[CommandName]--;
            }
        }
    }

    public bool IsCastingWithSpecie(PlayerBaseStats playerStats, CCSPlayerController player)
    {
        var dndPlayer = CommonMethods.RetrievePlayer(player);
        var classLevel = CommonMethods.RetrievePlayerClassLevel(player);
        var reqs = ClassSpecieRequirements
           .Where(r => 
               // The first line is seeing if the player IS the class, but isn't high enough level to cast that class's ability yet
               (r.DndClass == null || (r.DndClass != null && r.DndSpecie == null && (int) r.DndClass == dndPlayer.DndClassId && r.ClassLevel != null & r.ClassLevel > classLevel))  
               || (r.DndSpecie != null && (int)r.DndSpecie == dndPlayer.DndSpecieId && (r.SpecieLevel == null || CommonMethods.RetrievePlayerSpecieLevel(player) >= r.SpecieLevel))
           ).ToList();
        if (reqs.Any())                
            return true;            

        return false;
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
                if (req.DndClass == playerClass)
                    if (CommonMethods.RetrievePlayerClassLevel(player) >= (req.ClassLevel ?? 1))
                        return true;
            }
            
            if (req.DndSpecie != null && req.DndClass == null)
            {
                if (req.DndSpecie == playerSpecie)
                    if (CommonMethods.RetrievePlayerSpecieLevel(player) >= (req.SpecieLevel ?? 1))
                        return true;
            }

            if (req.DndSpecie != null && req.DndClass != null)
            {
                if(playerClass == req.DndClass && playerSpecie == req.DndSpecie)                    
                    if(CommonMethods.RetrievePlayerClassLevel(player) >= (req.ClassLevel ?? 1))
                        if (CommonMethods.RetrievePlayerSpecieLevel(player) >= (req.SpecieLevel ?? 1))                
                            return true;
            }
        }

        return false;
    }
    
    public abstract bool UseAbility(CCSPlayerController player, PlayerBaseStats playerStats, List<string> arguments);    
}
