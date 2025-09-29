using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;
using Dndcs2.timers;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.stats;

public class PlayerBaseStats
{
    public int Userid {get; private set;}
    public int MaxHealth { get; private set; } = 100;
    public int MaxMana { get; private set; } = 0;
    public int Mana { get; private set; }
    public float Speed { get; private set; } = 1.0f;
    public List<string> AllowedWeapons { get; private set; } = new();

    public PlayerBaseStats(int userid)
    {
        Userid = userid;
        Reset();        
    }

    public void Reset()
    {
        MaxHealth = 100;
        MaxMana = 0;
        Speed = 1.0f;
        AllowedWeapons = new List<string>();
        foreach(var weapon in Dndcs2.Weapons.Except(Dndcs2.Snipers))
            PermitWeapon(weapon);
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
        Mana += amount;
    }
    
    public void ChangeSpeed(float amount, float? duration = null)
    {
        Speed += amount;
        var player = Utilities.GetPlayerFromUserid(Userid);
        
        Server.NextFrame(() =>
        {
            player.PlayerPawn.Value.VelocityModifier = Speed;
            MessagePlayer(player, $"Changed speed to {Speed}");
        });
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
}