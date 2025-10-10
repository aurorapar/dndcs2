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
    public constants.DndClass? QueuedClass = null;
    public constants.DndSubClass? QueuedSubClass = null;
    public constants.DndSpecie? QueuedSpecie = null;
    public int MaxHealth { get; private set; } = 100;
    public int MaxMana { get; private set; } = 0;
    public int Mana { get; private set; }
    public double Speed { get; private set; } = 1.0f;
    public int AbilityCooldown { get; set; } = 0;
    public List<string> AllowedWeapons = new();
    public Dictionary<string, int> SpellLimitedUses { get; private set; } = new();
    public Dictionary<string, int> SpecieLimitedUses { get; private set; } = new();

    private PlayerStatRating _wisdomStat = PlayerStatRating.Low;
    private PlayerStatRating _strengthStat = PlayerStatRating.Low;
    private PlayerStatRating _dexterityStat = PlayerStatRating.Low;
    private PlayerStatRating _constitutionStat = PlayerStatRating.Low;
    private PlayerStatRating _charismaStat = PlayerStatRating.Low;
    private PlayerStatRating _intelligenceStat = PlayerStatRating.Low;
    
    public bool Bless;
    public bool Bane;
    public bool FaerieFire;
    public bool Wildshape;
    public int WildshapeHealth;
    public string OriginalModel;
    public int FlurryOfBlows;
    public Dictionary<int, List<string>> MonkHits = new();
    public List<int> FlurryMessages = new();
    public Vector? InfernoLocation;
    public int InfernoSpawnedTick;
    public Vector? FlashbangLocation;
    public int FlashbangSpawnedTick;
    public List<CChicken> Chickens = new();


    public PlayerBaseStats(int userid)
    {
        Userid = userid;
        Reset();        
    }

    public void Reset()
    {
        var player = Utilities.GetPlayerFromUserid(Userid);
        var dndPlayer = CommonMethods.RetrievePlayer(player);
        if (QueuedClass != null)
        {
            CommonMethods.ChangeClass(player, (constants.DndClass)QueuedClass);
            if (QueuedSubClass == null && dndPlayer.DndSubClassId != null)
            {
                if (Dndcs2.Instance.DndSubClassLookup[(constants.DndSubClass)dndPlayer.DndSubClassId]
                        .DndParentClassId != dndPlayer.DndClassId)
                {
                    MessagePlayer(player, "Your Sub Class has been removed because it did not match your Class.");
                    CommonMethods.ChangeSubClass(player, null);
                }
            }
        }

        if (QueuedSubClass != null)
        {
            var subClass = Dndcs2.Instance.DndSubClassLookup[QueuedSubClass.Value];
            if(subClass.DndParentClassId != dndPlayer.DndClassId)
            {
                QueuedSubClass = null;
                MessagePlayer(player, "Your Sub Class has been removed because it did not match your Class.");
                CommonMethods.ChangeSubClass(player, null);
            }
            else
                CommonMethods.ChangeSubClass(player, (constants.DndSubClass)QueuedSubClass);
        }

        if (QueuedSpecie != null)
            CommonMethods.ChangeSpecie(player, (constants.DndSpecie) QueuedSpecie);
        QueuedClass = null;
        QueuedSpecie = null;
        MaxHealth = 100;
        MaxMana = 0;
        Mana = 0;
        FlurryOfBlows = 0;
        Wildshape = false;
        OriginalModel = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState?.ModelName;
        MonkHits = new();
        FlurryMessages = new();
        if(Speed != 1)
            ChangeSpeed((Speed - 1) * -1);
        
        AllowedWeapons = new List<string>();
        AbilityCooldown = 0;
        SpellLimitedUses = new();
        SpecieLimitedUses = new();
        
        Chickens = new();
        InfernoLocation = null;
        Bless = false;
        Bane = false;
        FaerieFire = false;
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
    
    public void ChangeHealth(int amount, float? duration = null)
    {
        var player = Utilities.GetPlayerFromUserid(Userid);
        Server.NextFrame(() =>
        {
            var newHealth = Math.Min(MaxHealth - player.PlayerPawn.Value.Health, amount);
            player.PlayerPawn.Value.Health += newHealth;
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
        });
        
        if (duration.HasValue)
        {
            new GenericTimer(duration.Value, duration.Value, 1, () =>
            {
                ChangeHealth(amount * -1);
            });
        }
    }
    
    public void ChangeMaxMana(int amount, float? duration = null)
    {
        MaxMana += amount;
        ChangeMana(amount, duration);

        if (duration.HasValue)
        {
            new GenericTimer(duration.Value, duration.Value, 1, () =>
            {
                MaxMana -= amount;
                ChangeMana(amount * -1);
            });
        }
    }

    public void ChangeMana(int amount, float? duration = null)
    {
        Mana = Math.Min(MaxMana, Mana + amount);
        Mana = Math.Max(Mana, 0);
        if (duration.HasValue)
        {
            new GenericTimer(duration.Value, duration.Value, 1, () =>
            {
                ChangeMana(amount * -1);
            });
        }
    }
    
    public void ChangeSpeed(double amount, float? duration = null)
    {
        Speed += amount;
        var player = Utilities.GetPlayerFromUserid(Userid);
        Server.NextFrame(() =>
        {
            player.PlayerPawn.Value.VelocityModifier = (float)Speed;
            player.PlayerPawn.Value.MovementServices.Maxspeed = 260 * (float)Speed;
        });
        if (duration.HasValue)
        {
            new GenericTimer(duration.Value, duration.Value, 1, () =>
            {
                ChangeSpeed(-1 * amount);
            });
        }        
    }

    public bool CheckWeapon(string weapon)
    {
        return AllowedWeapons.Contains(weapon.ToLower());
    }

    public void PermitWeapon(string weapon, float? duration = null)
    {
        if(!AllowedWeapons.Contains(weapon.ToLower()))
            AllowedWeapons.Add(weapon.ToLower());
    }
    
    public void RestrictWeapon(string weapon, float? duration = null)
    {
        if(AllowedWeapons.Contains(weapon.ToLower()))
            AllowedWeapons.Remove(weapon.ToLower());
    }
    
    public void PermitWeapons(List<string> weapons, float? duration = null)
    {
        foreach (var weapon in weapons)
        {
            if (!AllowedWeapons.Contains(weapon.ToLower()))
                AllowedWeapons.Add(weapon.ToLower());
        }
    }
    
    public void RestrictWeapon(List<string> weapons, float? duration = null)
    {
        foreach (var weapon in weapons)
        {
            if (AllowedWeapons.Contains(weapon.ToLower()))
                AllowedWeapons.Remove(weapon.ToLower());
        }
    }

    public List<string> GetAllowedWeapons()
    {
        return AllowedWeapons.Select(w => w.ToUpper()).ToList();
    }

    public int GetPlayerStatValue(PlayerAbility statEnum)
    {
        PlayerStatRating statRating = ((Func<PlayerAbility, PlayerStatRating>)((saveStat) =>
        {
            switch (saveStat)
            {
                case PlayerAbility.Strength:
                    return _strengthStat;
                case PlayerAbility.Dexterity:
                    return _dexterityStat;
                case PlayerAbility.Constitution:
                    return _constitutionStat;
                case PlayerAbility.Intelligence:
                    return _intelligenceStat;
                case PlayerAbility.Wisdom:
                    return _wisdomStat;
                case PlayerAbility.Charisma:
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
        
        return stat + (Bless ? new Random().Next(0, 3) + 1 : 0) - (Bane ? new Random().Next(0, 3) + 1 : 0);
    }

    public int GetProficiencyBonus(int level)
    {
        int playerLevel = CommonMethods.RetrievePlayerClassLevel(Utilities.GetPlayerFromUserid(Userid));
        return (int) Math.Ceiling((double) playerLevel / 4 + 1);
    }

    public bool MakeDiceCheck(CCSPlayerController aggressor, PlayerAbility aggressorAbility, PlayerAbility victimAbility, bool advantage = false)
    {        
        var victim = Utilities.GetPlayerFromUserid(Userid);
        var aggressorStats = PlayerStats.GetPlayerStats(aggressor);
        var diceCheckTarget = 8 + aggressorStats.GetPlayerStatValue(aggressorAbility);
        string saveType = aggressorAbility.ToString().Split("Save")[0];
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
        if (diceRoll.Result + GetPlayerStatValue(victimAbility) >= diceCheckTarget)
        {
            MessagePlayer(victim, $"You succeeded a {saveType} Save DC of {diceCheckTarget}");
            MessagePlayer(aggressor, $"Someone succeeded on your {saveType} Save DC of {diceCheckTarget}");
            return true;
        }
       
        MessagePlayer(victim, $"You failed a {saveType} Save DC of {diceCheckTarget}");
        MessagePlayer(aggressor, $"Someone failed your {saveType} Save DC of {diceCheckTarget}");
        return false;
    }

    public void SetGoodStat(PlayerAbility stat)
    {
        switch (stat)
        {
            case PlayerAbility.Strength:
                _strengthStat = PlayerStatRating.High;
                return;
            case PlayerAbility.Dexterity:
                _dexterityStat = PlayerStatRating.High;
                return;
            case PlayerAbility.Constitution:
                _constitutionStat = PlayerStatRating.High;
                return;
            case PlayerAbility.Intelligence:
                _intelligenceStat = PlayerStatRating.High;
                return;
            case PlayerAbility.Wisdom:
                _wisdomStat = PlayerStatRating.High;
                return;
            case PlayerAbility.Charisma:
                _charismaStat = PlayerStatRating.High;
                return;
            default:
                throw new ArgumentException("Needed to supply a PlayerStat");
        };
    }
    
    public void SetAverageStat(PlayerAbility stat)
    {
        switch (stat)
        {
            case PlayerAbility.Strength:
                _strengthStat = PlayerStatRating.Average;
                return;
            case PlayerAbility.Dexterity:
                _dexterityStat = PlayerStatRating.Average;
                return;
            case PlayerAbility.Constitution:
                _constitutionStat = PlayerStatRating.Average;
                return;
            case PlayerAbility.Intelligence:
                _intelligenceStat = PlayerStatRating.Average;
                return;
            case PlayerAbility.Wisdom:
                _wisdomStat = PlayerStatRating.Average;
                return;
            case PlayerAbility.Charisma:
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

public enum PlayerAbility
{
    Strength,
    Dexterity,
    Constitution,
    Wisdom,
    Charisma,
    Intelligence
}