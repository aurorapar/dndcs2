using CounterStrikeSharp.API;

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
    }

    public void ChangeMaxHealth(int amount)
    {
        MaxHealth += amount;
        var player = Utilities.GetPlayerFromUserid(Userid);
        player.Health += amount;
    }
    
    public void ChangeMaxMana(int amount)
    {
        MaxMana += amount;
        Mana += amount;
    }

    public void ChangeMana(int amount)
    {
        Mana += amount;
    }
    
    public void ChangeSpeed(float amount)
    {
        Speed += amount;
        var player = Utilities.GetPlayerFromUserid(Userid);
        player.PlayerPawn.Value.VelocityModifier = Speed;
    }

    public void PermitWeapon(string weapon)
    {
        if(!AllowedWeapons.Contains(weapon))
            AllowedWeapons.Add(weapon);
    }
    
    public void RestrictWeapon(string weapon)
    {
        if(AllowedWeapons.Contains(weapon))
            AllowedWeapons.Remove(weapon);
    }
}