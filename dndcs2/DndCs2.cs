using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

using CounterStrikeSharp.API.Core;

namespace DndCs2;

public class DndCs2 : BasePlugin
{
    public override string ModuleName => "[Dungeons & Dragons CS2]";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "Aurora";
    public override string ModuleDescription  => "An overhaul that brings D&D Mechanics to CS2";
    
    private SqliteConnection _connection = null!;
    
    public override void Load(bool hotreload)
    {
        var basePath = System.IO.Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(Path.Join(ModuleDirectory, "appsettings.json"), optional: false, reloadOnChange: true).Build();

        // Read settings 
        var databaseName = configuration["Database:FileName"];
        Console.WriteLine($"Reading from database: {databaseName}");
    }
}