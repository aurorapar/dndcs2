using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Dndcs2.dtos;
using static Dndcs2.messages.DndMessages;


namespace Dndcs2.Sql;

public class DndcsContext : DbContext
{
    public DbSet<DndClass> DndClasses { get; set; }
    public DbSet<DndClassRequirement> DndClassRequirements { get; set; }
    
    public DbSet<DndSpecie> DndSpecies { get; set; }
    public DbSet<DndSpecieRequirement> DndSpecieRequirements { get; set; }
    
    public DbSet<DndPlayer> DndPlayers { get; set; }
    public DbSet<DndPlayerClassExperience> DndPlayerClassExperiences { get; set; }
    public DbSet<DndPlayerSpecieExperience> DndPlayerSpecieExperiences { get; set; }
    
    public DbSet<DndPlayerStats> DndPlayerStats { get; set; }
    public DbSet<DndExperienceLog> DndExperienceLogs { get; set; }

    public string DbPath { get; }

    public DndcsContext()
    {
        DbPath = Dndcs2.DatabaseLocation;
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={this.DbPath}");
        SQLitePCL.Batteries.Init();
    }

    public void EnsureCreated()
    {        
        Database.EnsureCreated();
        PrintMessageToConsole("Database Created!");
        Dndcs2.DndLogger.LogInformation("New database created");
    }
}