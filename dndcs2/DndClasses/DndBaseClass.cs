using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using static Dndcs2.constants.DndClassDescription;
using static Dndcs2.messages.DndMessages;
using static Dndcs2.DndClasses.SharedClassFeatures;
using Dndcs2.events;
using Dndcs2.dtos;
using Dndcs2.Sql;
using Dndcs2.stats;
using DndClass = Dndcs2.dtos.DndClass;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;


namespace Dndcs2.DndClasses;

public abstract class DndBaseClass : DndClass
{
    private static bool _spawnEventRegistered = false;
    public abstract PlayerStat GoodStat { get; }
    public abstract PlayerStat AverageStat { get; }
    public abstract PlayerStatRating HealthRating { get; }
    public abstract List<string> WeaponList { get; }
    
    public List<EventCallbackFeatureContainer> DndClassSpecieEvents { get; protected set; } = new();
    
    protected DndBaseClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        constants.DndClass dndClass, Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, (int) dndClass, dndClass.ToString().Replace("_", " "), 
            DndClassDescriptions[dndClass], dndClassRequirements)
    {
        Dndcs2.Instance.Log.LogInformation($"Created class {GetType().Name}");

        if (!_spawnEventRegistered)
        {
            _spawnEventRegistered = true;
            DndClassSpecieEvents.AddRange(new List<EventCallbackFeatureContainer>()
            {
                new AllClassesSpawn()
            });
        }
    }

    public static void RegisterClasses()
    {
        var dndClasses = new List<Tuple<constants.DndClass, Type>>()
        {
            new(constants.DndClass.Fighter, typeof(DndClasses.Fighter)),
            new(constants.DndClass.Rogue, typeof(DndClasses.Rogue)),
            new(constants.DndClass.Cleric, typeof(DndClasses.Cleric)),
            new(constants.DndClass.Wizard, typeof(DndClasses.Wizard)),
        };        
        
        var dndClassEnumType = typeof(constants.DndClass);
        foreach (var dndClassEnumCombo in dndClasses)
        {
            var dndClassEnum = dndClassEnumCombo.Item1;
            var dndClassType = dndClassEnumCombo.Item2;
            var constructor = dndClassType.GetConstructors();
            
            int dndClassId = (int)dndClassEnum;
            var dndClassRecord = CommonMethods.RetrieveDndClass(dndClassId);
            
            if (dndClassRecord == null)
            {
                DateTime creationTime = DateTime.UtcNow;
                string author = "D&D Initial Creation";
                bool enabled = true;
                string dndClassName = Enum.GetName(dndClassEnumType, dndClassEnum).Replace('_', ' ');
                try
                {
                    var newDndClass = constructor[0].Invoke(new object[]
                    {
                        author,
                        creationTime,
                        author,
                        creationTime,
                        enabled
                    });
                    CommonMethods.CreateNewDndClass((DndBaseClass) newDndClass);
                    Dndcs2.Instance.DndClassLookup[dndClassEnum] = (DndBaseClass) newDndClass;
                    Dndcs2.Instance.Log.LogInformation($"{dndClassName} added to database");
                }
                catch (Exception e)
                {
                    Dndcs2.Instance.Log.LogError($"Error registering class {dndClassEnum}");
                    Dndcs2.Instance.Log.LogError(e.ToString());
                    return;
                }
            }
            else
            {
                Collection<DndClassRequirement> classReqs = new();
                foreach (var classReq in dndClassRecord.DndClassRequirements)
                {
                    classReqs.Add(new DndClassRequirement(
                        classReq.CreatedBy,
                        classReq.CreateDate, 
                        classReq.UpdatedBy,
                        classReq.UpdatedDate,
                        classReq.Enabled, 
                        dndClassRecord.DndClassId,
                        classReq.DndRequiredClassId, 
                        classReq.DndRequiredClassLevel
                    ));
                }
                
                var newDndClass = constructor[0].Invoke(new object[]
                {
                    dndClassRecord.CreatedBy,
                    dndClassRecord.CreateDate, 
                    dndClassRecord.UpdatedBy,
                    dndClassRecord.UpdatedDate,
                    dndClassRecord.Enabled
                });
                Dndcs2.Instance.DndClassLookup[dndClassEnum] = (DndBaseClass) newDndClass;
                Dndcs2.Instance.Log.LogInformation($"{((DndBaseClass) newDndClass).DndClassName} loaded from database");
            }
        }        
    }
    
    public class AllClassesSpawn : EventCallbackFeature<EventPlayerSpawn>
    {
        public AllClassesSpawn() : 
            base(false, EventCallbackFeaturePriority.High, HookMode.Post, PlayerPostSpawn,null, null)
        {
            
        }

        public static HookResult PlayerPostSpawn(EventPlayerSpawn @event, GameEventInfo info, DndPlayer dndPlayer,
            DndPlayer? dndPlayerAttacker)
        {
            //Dndcs2.Instance.Log.LogInformation($"AllClassesSpawn called");
            if (@event.Userid == null || @event.Userid.ControllingBot || (int) @event.Userid.Team == 0)
                    return HookResult.Continue;

            //Dndcs2.Instance.Log.LogInformation($"AllClassesSpawn userid retrieved");
            var userid = (int)@event.Userid.UserId;

            Server.NextFrame(() =>
            {
                //Dndcs2.Instance.Log.LogInformation($"AllClassesSpawn nextframe started");
                var player = Utilities.GetPlayerFromUserid(userid);
                
                if (player == null)
                    return;
                if (player.ControllingBot || player.Team == 0)
                    return;
                
                var dndPlayer = CommonMethods.RetrievePlayer(player);
                var playerClassName = (constants.DndClass)dndPlayer.DndClassId;
                var playerClass = Dndcs2.Instance.DndClassLookup
                    [(constants.DndClass)dndPlayer.DndClassId];
                var classLevel = CommonMethods.RetrievePlayerClassLevel(player);
                var playerStats = PlayerStats.GetPlayerStats(player);

                MessagePlayer(player,
                    $"You have a good {playerClass.GoodStat} and average {playerClass.AverageStat} as a {playerClassName}");
                playerStats.SetGoodStat(playerClass.GoodStat);
                playerStats.SetAverageStat(playerClass.AverageStat);
                
                playerStats.PermitWeapons(Dndcs2.Equipment.ToList().Concat(playerClass.WeaponList).ToList());

                var bonusHealth = (int)playerStats.GetPlayerHealthPerLevel(playerClass.HealthRating) * classLevel;
                MessagePlayer(player,
                    $"You gained {bonusHealth} bonus health for being a Level {classLevel} {(constants.DndClass)dndPlayer.DndClassId}");
                playerStats.ChangeMaxHealth(bonusHealth);
                ShowSpells(player, dndPlayer, playerStats);
            });
            return HookResult.Continue;
        }
    }
}
