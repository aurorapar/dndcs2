using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Dndcs2.dice;
using Dndcs2.Sql;
using Dndcs2.timers;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.stats;

public class PlayerBaseStats
{
    public int Userid {get; private set;}
    public int MaxHealth { get; private set; } = 100;
    public int MaxMana { get; private set; } = 0;
    public int Mana { get; private set; }
    public double Speed { get; private set; } = 1.0f;
    public int AbilityCooldown { get; set; } = 0;
    public List<string> AllowedWeapons { get; private set; } = new();
    public Dictionary<string, int> SpellLimitedUses { get; private set; } = new();

    private PlayerStatRating _wisdomStat = PlayerStatRating.Low;
    private PlayerStatRating _strengthStat = PlayerStatRating.Low;
    private PlayerStatRating _dexterityStat = PlayerStatRating.Low;
    private PlayerStatRating _constitutionStat = PlayerStatRating.Low;
    private PlayerStatRating _charismaStat = PlayerStatRating.Low;
    private PlayerStatRating _intelligenceStat = PlayerStatRating.Low;
    
    public bool Guidance;
    public Vector? InfernoLocation;
    public int InfernoSpawnedTick { get; set; }
    public Vector? FlashbangLocation;
    public int FlashbangSpawnedTick { get; set; }


    public PlayerBaseStats(int userid)
    {
        Userid = userid;
        Reset();        
    }

    public void Reset()
    {
        Dndcs2.Instance.Log.LogInformation($"PlayerBaseStats Reset for {Utilities.GetPlayerFromUserid(Userid).PlayerName}");
        MaxHealth = 100;
        MaxMana = 0;
        Mana = 0;
        if(Speed != 1)
            ChangeSpeed((Speed - 1) * -1);
        
        AllowedWeapons = new List<string>();
        AbilityCooldown = 0;
        SpellLimitedUses = new();
        foreach(var weapon in Dndcs2.Weapons.Except(Dndcs2.Snipers))
            PermitWeapon(weapon);
        
        InfernoLocation = null;
        Guidance = false;
    }

    public void ChangeMaxHealth(int amount, float? duration = null)
    {
        MaxHealth += amount;
        var player = Utilities.GetPlayerFromUserid(Userid);
        if (player == null || player.PlayerPawn.Value == null)
            return;
        Server.NextFrame(() =>
        {
            player.PlayerPawn.Value.MaxHealth += amount;
            player.PlayerPawn.Value.Health += amount;
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iMaxHealth");
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
        });
        if (duration.HasValue)
        {
            new GenericTimer(duration.Value, duration.Value, 1, () =>
            {
                ChangeMaxHealth(-1 * amount);
            });
        }
    }
    
    public void ChangeMaxMana(int amount, float? duration = null)
    {
        MaxMana += amount;
        Mana += amount;

        if (duration.HasValue)
        {
            new GenericTimer(duration.Value, duration.Value, 1, () =>
            {
                MaxMana -= amount;
                Mana -= amount;
            });
        }
    }

    public void ChangeMana(int amount, float? duration = null)
    {
        Mana = Math.Min(MaxMana, Mana + amount);
    }
    
    public void ChangeSpeed(double amount, float? duration = null)
    {
        Speed += amount;
        var player = Utilities.GetPlayerFromUserid(Userid);
        
        Server.NextFrame(() => player.PlayerPawn.Value.VelocityModifier = (float) Speed);
        if (duration.HasValue)
        {
            new GenericTimer(duration.Value, duration.Value, 1, () =>
            {
                ChangeSpeed(-1 * amount);
            });
        }
        
        
    }

    public void PermitWeapon(string weapon, float? duration = null)
    {
        if(!AllowedWeapons.Contains(weapon))
            AllowedWeapons.Add(weapon);
    }
    
    public void RestrictWeapon(string weapon, float? duration = null)
    {
        if(AllowedWeapons.Contains(weapon))
            AllowedWeapons.Remove(weapon);
    }
    
    public void PermitWeapons(List<string> weapons, float? duration = null)
    {
        foreach (var weapon in weapons)
        {
            if (!AllowedWeapons.Contains(weapon))
                AllowedWeapons.Add(weapon);
        }
    }
    
    public void RestrictWeapon(List<string> weapons, float? duration = null)
    {
        foreach (var weapon in weapons)
        {
            if (AllowedWeapons.Contains(weapon))
                AllowedWeapons.Remove(weapon);
        }
    }

    public int GetPlayerStatValue(PlayerStat statEnum)
    {
        PlayerStatRating statRating = ((Func<PlayerStat, PlayerStatRating>)((saveStat) =>
        {
            switch (saveStat)
            {
                case PlayerStat.Strength:
                    return _strengthStat;
                case PlayerStat.Dexterity:
                    return _dexterityStat;
                case PlayerStat.Constitution:
                    return _constitutionStat;
                case PlayerStat.Intelligence:
                    return _intelligenceStat;
                case PlayerStat.Wisdom:
                    return _wisdomStat;
                case PlayerStat.Charisma:
                    return _charismaStat;
                default:
                    throw new ArgumentException("Needed to supply a PlayerStat");
            };
        }))(statEnum);

        var player = Utilities.GetPlayerFromUserid(Userid);
        int playerLevel = CommonMethods.RetrievePlayerClassLevel(player);
        int proficiencyBonus = GetProficiencyBonus(playerLevel);
        int stat = ((Func<PlayerStatRating, int, int>)((rating, level) => 
        {
            switch (rating)
            {
                case PlayerStatRating.Low:
                    return -1;
                case PlayerStatRating.Average:
                    return 2 + proficiencyBonus;
                case PlayerStatRating.High:
                    return 3 + int.Max(level / 4, 2) + proficiencyBonus;
                default:
                    throw new ArgumentException("Needed to supply a PlayerStatRating");
            }
        }))(statRating, playerLevel);
        
        return stat + (Guidance ? new Random().Next(0, 3) + 1 : 0);
    }

    public int GetProficiencyBonus(int level)
    {
        int playerLevel = CommonMethods.RetrievePlayerClassLevel(Utilities.GetPlayerFromUserid(Userid));
        return (int) Math.Ceiling((double) playerLevel / 4 + 1);
    }

    public bool MakeDiceCheck(CCSPlayerController aggressor, PlayerStat aggressorStat, PlayerStat victimStat, bool advantage = false)
    {        
        var victim = Utilities.GetPlayerFromUserid(Userid);
        var aggressorStats = PlayerStats.GetPlayerStats(aggressor);
        var diceCheckTarget = 8 + aggressorStats.GetPlayerStatValue(aggressorStat);
        string saveType = aggressorStat.ToString().Split("Save")[0];
        var diceRoll = new DieRoll(sides:20, amount:1, advantage: advantage);
        
        if (diceRoll.CriticalFailure)
        {
            MessagePlayer(victim, $"You critically failed a {saveType} Save DC of {diceCheckTarget}!");
            MessagePlayer(aggressor, $"Someone critically failed on your {saveType} Save DC of {diceCheckTarget}!");
            return false;
        }
        if (diceRoll.Critical)
        {
            MessagePlayer(victim, $"You critically succeeded a {saveType} Save DC of {diceCheckTarget}!");
            MessagePlayer(aggressor, $"Someone critically succeeded on your {saveType} Save DC of {diceCheckTarget} :(");
            return true;
        }

        var diceResult = diceRoll.Result;
        if (diceRoll.Result + GetPlayerStatValue(victimStat) >= diceCheckTarget)
        {
            MessagePlayer(victim, $"You succeeded a {saveType} Save DC of {diceCheckTarget}");
            MessagePlayer(aggressor, $"Someone succeeded on your {saveType} Save DC of {diceCheckTarget}");
            return true;
        }
       
        MessagePlayer(victim, $"You failed a {saveType} Save DC of {diceCheckTarget}");
        MessagePlayer(aggressor, $"Someone failed your {saveType} Save DC of {diceCheckTarget}");
        return false;
    }

    public void SetGoodStat(PlayerStat stat)
    {
        switch (stat)
        {
            case PlayerStat.Strength:
                _strengthStat = PlayerStatRating.High;
                return;
            case PlayerStat.Dexterity:
                _dexterityStat = PlayerStatRating.High;
                return;
            case PlayerStat.Constitution:
                _constitutionStat = PlayerStatRating.High;
                return;
            case PlayerStat.Intelligence:
                _intelligenceStat = PlayerStatRating.High;
                return;
            case PlayerStat.Wisdom:
                _wisdomStat = PlayerStatRating.High;
                return;
            case PlayerStat.Charisma:
                _charismaStat = PlayerStatRating.High;
                return;
            default:
                throw new ArgumentException("Needed to supply a PlayerStat");
        };
    }
    
    public void SetAverageStat(PlayerStat stat)
    {
        switch (stat)
        {
            case PlayerStat.Strength:
                _strengthStat = PlayerStatRating.Average;
                return;
            case PlayerStat.Dexterity:
                _dexterityStat = PlayerStatRating.Average;
                return;
            case PlayerStat.Constitution:
                _constitutionStat = PlayerStatRating.Average;
                return;
            case PlayerStat.Intelligence:
                _intelligenceStat = PlayerStatRating.Average;
                return;
            case PlayerStat.Wisdom:
                _wisdomStat = PlayerStatRating.Average;
                return;
            case PlayerStat.Charisma:
                _charismaStat = PlayerStatRating.Average;
                return;
            default:
                throw new ArgumentException("Needed to supply a PlayerStat");
        };
    }

    public double GetPlayerHealthPerLevel(PlayerStatRating healthRating)
    {
        var conhealthModifier = GetPlayerConstitutionHealthBenefit();
        switch (healthRating)
        {
            case PlayerStatRating.Low:
                return 1.5 + conhealthModifier;
            case PlayerStatRating.Average:
                return 2.0 + conhealthModifier;
            case PlayerStatRating.High:
                return 2.5 + conhealthModifier;
            default:
                throw new ArgumentException("Constitution had no rating");
        }
    }

    public double GetPlayerConstitutionHealthBenefit()
    {
        var player = Utilities.GetPlayerFromUserid(Userid);
        var level = CommonMethods.RetrievePlayerClassLevel(player);
        switch (_constitutionStat)
        {
            case PlayerStatRating.Low:
                return -1;
            case PlayerStatRating.Average:
                return 2.0;
            case PlayerStatRating.High:
                return 3 + int.Max(level / 4, 2);
            default:
                throw new ArgumentException("Constitution had no rating");
        }
    }
    
    public enum PlayerStatRating
    {
        Low,
        Average,
        High
    }
}

public enum PlayerStat
{
    Strength,
    Dexterity,
    Constitution,
    Wisdom,
    Charisma,
    Intelligence
}