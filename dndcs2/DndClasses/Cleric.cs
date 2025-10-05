using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using static Dndcs2.DndClasses.SharedClassFeatures;
using static Dndcs2.messages.DndMessages;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Cleric : DndBaseClass
{
    public override PlayerStat GoodStat { get; } = PlayerStat.Wisdom;
    public override PlayerStat AverageStat { get; } = PlayerStat.Charisma;
    public override PlayerStatRating HealthRating { get; }= PlayerStatRating.Average;

    public override List<string> WeaponList { get; } = Dndcs2.Weapons
        .Except(Dndcs2.Snipers)
        .Except(Dndcs2.Rifles)
        .Except(Dndcs2.MGs)
        .Concat(new List<string>(){"galilar", "famas"})
        .ToList();
    
    public Cleric(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndClass.Cleric,new Collection<DndClassRequirement>())
    {
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new ClericSpawn(),
            new HealingWord(),
        });
    }
    
    public class ClericSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public ClericSpawn() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                constants.DndClass.Cleric, null)
        {
            
        }

        public static HookResult PlayerPostSpawn(EventPlayerSpawn @event, GameEventInfo info, DndPlayer dndPlayer,
            DndPlayer? dndPlayerAttacker)
        {
            var userid = (int) @event.Userid.UserId;
            Server.NextFrame(() =>
            {
                var player = Utilities.GetPlayerFromUserid(userid);
                if (player == null)
                    return;
                var dndPlayer = CommonMethods.RetrievePlayer(player);
                if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Cleric)
                    return;

                var playerStats = PlayerStats.GetPlayerStats(dndPlayer);
                var clericLevel = CommonMethods.RetrievePlayerClassLevel(player);

                AddFullCasterMana(clericLevel, playerStats, dndPlayer);  
                MessagePlayer(player, "Healing Word: Your shots will heal allies as long as you have mana on a 1-for-1 basis");
                MessagePlayer(player, "Healing Word:     Lowest value between their missing HP, your mana, and 20");
            });            
            return HookResult.Continue;
        }
    }
    
    public class HealingWord : EventCallbackFeature<EventPlayerHurt>
    {
        public HealingWord() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Pre, PlayerHurtPre, 
                constants.DndClass.Cleric, null)
        {
            
        }

        public static HookResult PlayerHurtPre(EventPlayerHurt @event, GameEventInfo info, DndPlayer dndPlayerVictim,
            DndPlayer dndPlayerAttacker)
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;
            var attackerClassEnum = (constants.DndClass) dndPlayerAttacker.DndClassId;
            
            if(attackerClassEnum != constants.DndClass.Cleric )
                return HookResult.Continue;
            
            if (attacker.Team != victim.Team)
                return HookResult.Continue;

            var playerStats = PlayerStats.GetPlayerStats(attacker);
            if (playerStats.Mana > 0)
            {
                var victimStats = PlayerStats.GetPlayerStats(victim);
                var healing = Math.Min(playerStats.Mana, @event.DmgHealth);
                
                @event.Health += @event.DmgHealth;
                victim.PlayerPawn.Value.Health = @event.Health;
                @event.DmgHealth = 0;
                
                healing = Math.Min(victimStats.MaxHealth - victim.PlayerPawn.Value.Health, healing);
                healing = Math.Min(20, healing);                
                
                if (healing < 1)
                    return HookResult.Continue;
                
                victimStats.ChangeHealth(healing);

                MessagePlayer(attacker, $"You healed {victim.PlayerName} for {healing}HP!");
                MessagePlayer(victim, $"{attacker.PlayerName} healed you for {healing}HP!");
                playerStats.ChangeMana(-healing);
            }

            return HookResult.Changed;
        }
    }
}