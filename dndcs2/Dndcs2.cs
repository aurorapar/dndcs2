using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Dndcs2.events.eventstack;
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

        EventFactory.RegisterEventCallbacks();        
        
        DndLogger.LogInformation("Loaded plugin");
    }

    public override void Unload(bool hotreload)
    {
        DndLogger.LogInformation("Unloaded plugin");
    }
}