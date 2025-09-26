using System.Collections.ObjectModel;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Core;
using Dndcs2.DndClasses;
using Dndcs2.DndSpecies;
using static Dndcs2.constants.DndClassDescription;
using static Dndcs2.constants.DndSpecieDescription;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;

namespace Dndcs2;

public partial class Dndcs2 : BasePlugin
{
    public override string ModuleName => "[Dungeons & Dragons CS2]";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "Aurora";
    public override string ModuleDescription  => "An overhaul that brings D&D Mechanics to CS2";
    public static Dndcs2 Instance { get; private set; }
    public static string DatabaseLocation { get; private set; }
    public static ILogger DndLogger { get; }
    public Dictionary<constants.DndClass, DndClass> DndClassLookup { get; private set; } = new();
    public Dictionary<constants.DndSpecie, DndSpecie> DndSpecieLookup { get; private set; } = new();
    
    private CCSGameRules? _gameRules;
    private bool _gameRulesInitialized;

    static Dndcs2()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        DndLogger = factory.CreateLogger(nameof(Dndcs2));

        var databaseName = "dndcs2.db";
        var working_folder = Path.Join(Environment.CurrentDirectory, "..", "..", "csgo", "dndcs2_database");
        if(!Directory.Exists(working_folder))
            Directory.CreateDirectory(working_folder);
        DatabaseLocation = Path.Join(working_folder, databaseName);
    }
    
    public override void Load(bool hotreload)
    {
        Instance = this;
        
        using(var connection = new DndcsContext())
            connection.EnsureCreated();

        RegisterListeners(hotreload);
        RegisterCommands();
        RegisterEventCallbacks();
        RegisterClassesSpecies();
        
        DndLogger.LogInformation("Loaded plugin");
    }

    public void RegisterListeners(bool hotReload)
    {
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnMapStart>(OnMapStartHandler);

        if (hotReload)
        {
            InitializeGameRules();
        }
    }
    
    public void OnMapStartHandler(string mapName)
    {
        _gameRules = null;
        _gameRulesInitialized = false;
    }
    
    private void InitializeGameRules()
    {
        if (_gameRulesInitialized) return;
            
        var gameRulesProxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
        _gameRules = gameRulesProxy?.GameRules;
        _gameRulesInitialized = _gameRules != null;
    }
    
    private void OnTick()
    {
        if (!_gameRulesInitialized)
        {
            InitializeGameRules();
            return;
        }

        if (_gameRules != null)
        {
            _gameRules.GameRestart = _gameRules.RestartRoundTime < Server.CurrentTime;
        }
    }

    public void RegisterClassesSpecies()
    {
        var dndClasses = new List<Tuple<constants.DndClass, Type>>()
        {
            new Tuple<constants.DndClass, Type>(constants.DndClass.Fighter, typeof(DndClasses.Fighter))
        };
        var dndSpecies = new List<Tuple<constants.DndSpecie, Type>>()
        {
            new Tuple<constants.DndSpecie, Type>(constants.DndSpecie.Human, typeof(DndSpecies.Human))
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
                var newDndClass = constructor[0].Invoke(new object[]
                {
                    author, 
                    creationTime, 
                    author, 
                    creationTime, 
                    enabled, 
                    dndClassName, 
                    dndClassDescription, 
                    classReqs
                });
                CommonMethods.CreateNewDndClass((DndBaseClass) newDndClass);
                DndClassLookup[dndClassEnum] = (DndBaseClass) newDndClass;
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
                    dndClassRecord.Enabled, 
                    dndClassRecord.DndClassName, 
                    dndClassRecord.DndClassDescription, 
                    classReqs
                });
                DndClassLookup[dndClassEnum] = (DndBaseClass) newDndClass;
            }
        }
        
        var dndSpecieEnumType = typeof(constants.DndSpecie);
        foreach (var dndSpecieEnumTypeCombo in dndSpecies)
        {
            var dndSpecieEnum = dndSpecieEnumTypeCombo.Item1;
            var dndSpecieType = dndSpecieEnumTypeCombo.Item2;
            var constructor = dndSpecieType.GetConstructors();
            
            int dndSpecieId = (int)dndSpecieEnum;
            var dndSpecieRecord = CommonMethods.RetrieveDndSpecie(dndSpecieId);
            
            if (dndSpecieRecord == null)
            {
                DateTime creationTime = DateTime.UtcNow;
                string author = "D&D Initial Creation";
                bool enabled = true;
                string dndSpecieName = Enum.GetName(dndSpecieEnumType, dndSpecieEnum).Replace('_', ' ');
                string dndSpecieDescription = DndSpecieDescriptions[dndSpecieEnum];
                var specieReqs = new Collection<DndSpecieRequirement>();
                var newDndSpecie = constructor[0].Invoke(new object[]
                {
                    author, 
                    creationTime, 
                    author, 
                    creationTime, 
                    enabled, 
                    dndSpecieName, 
                    dndSpecieDescription, 
                    0, //TODO: define level adjustment somewhere
                    specieReqs
                });
                CommonMethods.CreateNewDndSpecie((DndBaseSpecie) newDndSpecie);
                DndSpecieLookup[dndSpecieEnum] = (DndBaseSpecie) newDndSpecie;
            }
            else
            {
                Collection<DndSpecieRequirement> specieReqs = new();
                foreach (var specieReq in dndSpecieRecord.DndSpecieRequirements)
                {
                    specieReqs.Add(new DndSpecieRequirement(
                        specieReq.CreatedBy,
                        specieReq.CreateDate, 
                        specieReq.UpdatedBy,
                        specieReq.UpdatedDate,
                        specieReq.Enabled, 
                        dndSpecieRecord.DndSpecieId, 
                        specieReq.DndRequiredSpecieId,
                        specieReq.DndRequiredSpecieLevel
                    ));
                }
                
                var newDndSpecie = constructor[0].Invoke(new object[]
                {
                    dndSpecieRecord.CreatedBy,
                    dndSpecieRecord.CreateDate, 
                    dndSpecieRecord.UpdatedBy,
                    dndSpecieRecord.UpdatedDate,
                    dndSpecieRecord.Enabled, 
                    dndSpecieRecord.DndSpecieName, 
                    dndSpecieRecord.DndSpecieDescription, 
                    dndSpecieRecord.DndSpecieLevelAdjustment,
                    specieReqs
                });
                DndSpecieLookup[dndSpecieEnum] = (DndBaseSpecie) newDndSpecie;
            }
        }
    }

    public override void Unload(bool hotreload)
    {
        var roundEndEvent = (RoundEnd) DndEvent<EventRoundEnd>.RetrieveEvent<EventRoundEnd>();
        foreach (var player in Utilities.GetPlayers())
        {
            var dndPlayer = CommonMethods.RetrievePlayer(player);
            CommonMethods.TrackPlayerLogout(dndPlayer, "Mod unloading");
            roundEndEvent.DoDefaultPostCallback(5, player, dndPlayer);    
        }
        
        using(var context = CommonMethods.CreateContext())
            CommonMethods.SaveChanges(context);
        DndLogger.LogInformation("Unloaded plugin");
    }
}