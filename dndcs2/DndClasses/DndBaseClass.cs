using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static Dndcs2.constants.DndClassDescription;
using static Dndcs2.messages.DndMessages;
using Dndcs2.events;
using Dndcs2.dtos;
using Dndcs2.Sql;
using Dndcs2.stats;
using DndClass = Dndcs2.dtos.DndClass;
using PlayerStatRating = Dndcs2.stats.PlayerBaseStats.PlayerStatRating;


namespace Dndcs2.DndClasses;

public abstract class DndBaseClass : DndClass
{
    public PlayerStat GoodStat { get; private set; }
    public PlayerStat AverageStat { get; private set; }
    public PlayerStatRating HealthRating { get; private set; }
    private static bool _registeredClassSpawnEvent = false;
    
    public List<EventCallbackFeatureContainer> DndClassSpecieEvents { get; protected set; } = new();
    
    protected DndBaseClass(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled, 
        constants.DndClass dndClass, PlayerStat goodStat, PlayerStat averageStat, PlayerStatRating healthRating, 
        Collection<DndClassRequirement> dndClassRequirements) : 
        base(createdBy, createDate, updatedBy, updatedDate, enabled, (int) dndClass, dndClass.ToString().Replace("_", " "), 
            DndClassDescriptions[dndClass], dndClassRequirements)
    {
        Dndcs2.Instance.Log.LogInformation($"Created class {GetType().Name}");
        GoodStat = goodStat;
        AverageStat = averageStat;
        HealthRating = healthRating;
        
        DndClassSpecieEvents.AddRange( new List<EventCallbackFeatureContainer>() {
            
        });

        if (!_registeredClassSpawnEvent)
        {
            Dndcs2.Instance.Log.LogInformation($"Registering universal class spawn posthook");
            _registeredClassSpawnEvent = true;
            Dndcs2.Instance.RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
            {
                if (@event.Userid == null || @event.Userid.ControllingBot)
                    return HookResult.Continue;

                var userid = (int)@event.Userid.UserId;

                Server.NextFrame(() =>
                {
                    var player = Utilities.GetPlayerFromUserid(userid);
                    if (player == null)
                        return;
                    var dndPlayer = CommonMethods.RetrievePlayer(player);
                    var playerClassName = (constants.DndClass)dndPlayer.DndClassId;
                    var playerClass = Dndcs2.Instance.DndClassLookup
                        [(constants.DndClass)dndPlayer.DndClassId];
                    var classLevel = CommonMethods.RetrievePlayerClassLevel(player);
                    var playerStats = PlayerStats.GetPlayerStats(player);

                    MessagePlayer(player,
                        $"You have a good {playerClass.GoodStat} and average {playerClass.AverageStat} save as a {playerClassName}");
                    playerStats.SetGoodStat(playerClass.GoodStat);
                    playerStats.SetAverageStat(playerClass.AverageStat);

                    var bonusHealth = (int)playerStats.GetPlayerHealthPerLevel(HealthRating) * classLevel;
                    MessagePlayer(player,
                        $"You gained {bonusHealth} bonus health for being a Level {classLevel} {(constants.DndClass)dndPlayer.DndClassId}");
                    playerStats.ChangeMaxHealth(bonusHealth);
                });
                return HookResult.Continue;
            });
        }
    }

    public static void RegisterClasses()
    {
        var dndClasses = new List<Tuple<constants.DndClass, Type>>()
        {
            new Tuple<constants.DndClass, Type>(constants.DndClass.Fighter, typeof(DndClasses.Fighter)),
            new Tuple<constants.DndClass, Type>(constants.DndClass.Rogue, typeof(DndClasses.Rogue)),
            new Tuple<constants.DndClass, Type>(constants.DndClass.Cleric, typeof(DndClasses.Cleric)),
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
                string dndClassDescription = DndClassDescriptions[dndClassEnum];
                var classReqs = new Collection<DndClassRequirement>();
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
                    Dndcs2.Instance.Log.LogInformation($"{((DndBaseClass) newDndClass).DndClassName} added to database");
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
}
