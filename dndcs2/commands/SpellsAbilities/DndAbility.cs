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
    
    public int ManaCost { get; } = 0;
    public int? LimitedUses { get; } = null;
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
            new Mana(),
            new Spells(),
            new Abilities(),
            new Guidance(),
            new ColorSpray(),
            new Bane(),
            new GoodChicken(),
            new FaerieFire(),
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
            return;
        
        var playerStats = PlayerStats.GetPlayerStats(player);

        bool castingWithSpecie = IsCastingWithSpecie(playerStats, player);
        if (castingWithSpecie)
        {
            if(!playerStats.SpecieLimitedUses.ContainsKey(CommandName))
                playerStats.SpecieLimitedUses.Add(CommandName, (int) SpecieLimitedUses);
            if (playerStats.SpecieLimitedUses[CommandName] <= 0)
            {
                MessagePlayer(player, $"You have run out of uses for {CommandName}!");
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
            if (playerStats.SpellLimitedUses[CommandName] <= 0)
            {
                MessagePlayer(player, $"You have run out of uses for {CommandName}!");
                return;
            }
        }

        if (UseAbility(player, playerStats, arguments))
        {
            if (castingWithSpecie)            
                playerStats.SpecieLimitedUses[CommandName]--;            
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
        var specieLevel = CommonMethods.RetrievePlayerSpecieLevel(player);
        var meetsClassRequirements = ClassSpecieRequirements.Where(r => r.DndClass != null && dndPlayer.DndClassId == (int) r.DndClass && r.DndSpecie == null && (r.ClassLevel == null || r.ClassLevel <= classLevel)).ToList();
        meetsClassRequirements.AddRange(
            ClassSpecieRequirements.Where(r => 
                        r.DndClass != null & r.DndSpecie != null 
                        && (int) r.DndClass == dndPlayer.DndClassId 
                        && (int) r.DndSpecie == dndPlayer.DndSpecieId
                        && (r.ClassLevel == null || r.ClassLevel <= classLevel)
                        && (r.SpecieLevel == null || r.SpecieLevel <= specieLevel)) 
        );
        var meetsSpecieRequirements = ClassSpecieRequirements.Where(r => r.DndSpecie != null && r.DndClass == null && (r.SpecieLevel == null || r.SpecieLevel <= specieLevel)).ToList();
        
        //
        // foreach (var r in meetsClassRequirements)
        // {
        //     Dndcs2.Instance.Log.LogInformation($"{r.DndClass?.ToString()} {r.DndSpecie?.ToString()}");
        //     Dndcs2.Instance.Log.LogInformation($"{!meetsClassRequirements.Any() && meetsSpecieRequirements.Any()}");
        // }
        
        //Dndcs2.Instance.Log.LogInformation($"Class reqs? {meetsSpecieRequirements.Any()}");
        //Dndcs2.Instance.Log.LogInformation($"Specie reqs? {meetsClassRequirements.Any()}");
        
        if (!meetsClassRequirements.Any() && meetsSpecieRequirements.Any())
            return true;

        return false;
    }

    public bool CheckClassSpecieRequirements(CCSPlayerController player)
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
