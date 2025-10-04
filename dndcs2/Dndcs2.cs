using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using Dndcs2.DndClasses;
using Dndcs2.DndSpecies;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.log;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2;

public partial class Dndcs2 : BasePlugin
{
    public override string ModuleName => "[Dungeons & Dragons CS2]";
    public override string ModuleVersion => "0.0.5";
    public override string ModuleAuthor => "Aurora";
    public override string ModuleDescription  => "An overhaul that brings D&D Mechanics to CS2";
    public static Dndcs2 Instance { get; private set; }
    public static DateTime RoundTime { get; set; }
    public string DatabaseLocation { get; private set; }
    public readonly DndLog Log;
    public Dictionary<constants.DndClass, DndBaseClass> DndClassLookup { get; private set; } = new();
    public Dictionary<constants.DndSpecie, DndBaseSpecie> DndSpecieLookup { get; private set; } = new();
    public List<PlayerBaseStats> PlayerBaseStats { get; private set; } = new();
    
    private CCSGameRules? _gameRules;
    private bool _gameRulesInitialized;
    
    public Dndcs2(DndLog dndLog)
    {
        Log = dndLog;
    }
    
    public override void Load(bool hotreload)
    {
        Instance = this;
        
        var databaseName = "dndcs2.db";
        var working_folder = Path.Join(Environment.CurrentDirectory, "..", "..", "csgo", "dndcs2_database");
        if(!Directory.Exists(working_folder))
            Directory.CreateDirectory(working_folder);
        DatabaseLocation = Path.Join(working_folder, databaseName);
        
        using(var connection = new DndcsContext())
            connection.EnsureCreated();

        RegisterListeners(hotreload);
        RegisterEventCallbacks();
        
        DndBaseClass.RegisterClasses();
        DndBaseSpecie.RegisterSpecies();
        RegisterCommands();
        
        Log.LogInformation("Loaded plugin");
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

    public override void Unload(bool hotreload)
    {
        var roundEndEvent = (RoundEnd) DndEvent<EventRoundEnd>.RetrieveEvent<EventRoundEnd>();
        foreach (var player in Utilities.GetPlayers())
        {
            var dndPlayer = CommonMethods.RetrievePlayer(player);
            CommonMethods.TrackPlayerLogout(dndPlayer, "Mod unloading");
            roundEndEvent.DoDefaultPostCallback(5, player, dndPlayer);    
        }
        
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamageHook.OnTakeDamage, HookMode.Pre);
        
        using(var context = CommonMethods.CreateContext())
            CommonMethods.SaveChanges(context);
        Log.LogInformation("Unloaded plugin");
    }
}