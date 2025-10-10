using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using static Dndcs2.DndClasses.SharedClassFeatures;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.DndClasses;

public class Wizard : DndBaseClass
{
    public override PlayerAbility GoodStat { get; } = PlayerAbility.Intelligence;
    public override PlayerAbility AverageStat { get; } = PlayerAbility.Wisdom;
    public override PlayerStatRating HealthRating { get; }= PlayerStatRating.Low;

    public override List<string> WeaponList { get; } = Dndcs2.Weapons
        .Except(Dndcs2.Snipers)
        .Except(Dndcs2.Rifles)
        .Except(Dndcs2.MGs)
        .Except(Dndcs2.Shotguns)
        .Except(Dndcs2.SMGs)
        .Except(new List<string>() {"vest", "vesthelm"})
        .ToList();
    
    public Wizard(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled) :
        base(createdBy, createDate, updatedBy, updatedDate, enabled, constants.DndClass.Wizard,new Collection<DndClassRequirement>())
    {
        
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            new WizardSpawn()
        });
    }    
    
    public class WizardSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public WizardSpawn() : 
            base(false, EventCallbackFeaturePriority.Medium, HookMode.Post, PlayerPostSpawn, 
                constants.DndClass.Wizard, null)
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
                if ((constants.DndClass)dndPlayer.DndClassId != constants.DndClass.Wizard)
                    return;

                var playerStats = PlayerStats.GetPlayerStats(dndPlayer);
                var wizardLevel = CommonMethods.RetrievePlayerClassLevel(player);
                
                AddFullCasterMana(wizardLevel, playerStats, dndPlayer);  
            });            
            return HookResult.Continue;
        }
    }
}