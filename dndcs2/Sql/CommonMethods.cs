using Dndcs2.dtos;
using Microsoft.Extensions.Logging;
using DndClass = Dndcs2.dtos.DndClass;

namespace Dndcs2.Sql;

public static class CommonMethods
{
    public static DndcsContext CreateContext()
    {
        return new DndcsContext();
    }
    
    public static DndPlayer? RetrievePlayer(int accountId, bool atConnectionTime)
    {
        // Players should *always* be created at connection. If that does not occur, I WANT any call to this method to
        // fail hard, not gracefully. Ignore any warnings caused at any time OTHER than testing for null values at 
        // creation
        using (var connection = CreateContext())
        {
            var candidates = connection.DndPlayers
                .Where(p => p.DndPlayerAccountId == accountId && p.Enabled == true);
            
            return candidates.FirstOrDefault();            
        }
    }
    
    public static DndPlayer RetrievePlayer(int accountId)
    {
        // Players should *always* be created at connection. If that does not occur, I WANT any call to this method to
        // fail hard, not gracefully. Ignore any warnings caused at any time OTHER than testing for null values at 
        // creation
        using (var connection = CreateContext())
        {
            var candidates = connection.DndPlayers
                .Where(p => p.DndPlayerAccountId == accountId && p.Enabled == true);
            
            return candidates.First();
        }
    }

    public static DndSpecieProgress? RetrieveSpecieProgress(int accountId)
    {
        DndPlayer? player =  RetrievePlayer(accountId);
        if (player == null)
            return null;
         
        using (var connection = CreateContext())
        {
            var candidates = connection.DndSpecieProgresses
                .Where(s => s.DndPlayerId == player.DndPlayerId && 
                            s.DndSpecieExperienceId == player.DndSpecieId && s.Enabled == true);
            return candidates.FirstOrDefault();
        }
    }
    
    public static DndClassProgress? RetrievePlayerClassProgress(int accountId)
    {
        DndPlayer? player =  RetrievePlayer(accountId);
        if (player == null)
            return null;
         
        using (var connection = CreateContext())
        {
            var candidates = connection.DndClassProgresses
                .Where(s => s.DndPlayerId == player.DndPlayerId && s.DndClassId == player.DndClassId &&
                            s.Enabled == true);
            return candidates.FirstOrDefault();
        }
    }
    
    public static DndClassProgress? RetrievePlayerClassProgress(DndPlayer player)
    {         
        using (var connection = CreateContext())
        {
            var candidates = connection.DndClassProgresses
                .Where(s => s.DndPlayerId == player.DndPlayerId && s.DndClassId == player.DndClassId &&
                            s.Enabled == true);
            return candidates.FirstOrDefault();
        }
    }
    
    public static DndPlayer CreateNewPlayer(int accountId, string creator)
    {
        var dndPlayer = new DndPlayer(
            creator,
            DateTime.UtcNow,
            creator,
            DateTime.UtcNow,
            true,
            accountId,
            // TODO: Random starting gold? Setting?
            0,
            (int)constants.DndClass.Fighter,
            (int)constants.DndSpecie.Human,
            DateTime.UtcNow
        );
        
        using (var connection = CreateContext())
        {
            connection.DndPlayers.Add(dndPlayer);    
            SaveChanges(connection);
        }

        Dndcs2.DndLogger.LogInformation($"Player {accountId} created");
        return dndPlayer;
    }

    public static DndPlayer TrackPlayerLogin(DndPlayer dndPlayer, DateTime loginTime, string creator)
    {
        dndPlayer.LastConnected = loginTime;
        dndPlayer.UpdatedDate = loginTime;
        dndPlayer.UpdatedBy = creator;
        using (var connection = CreateContext())
            SaveChanges(connection);
             
        return dndPlayer;
    }

    public static DndClass? RetrieveDndClass(int dndClassId)
    {
        using (var connection = CreateContext())
        {
            var candidates = connection.DndClasses
                .Where(c => c.DndClassId == dndClassId && c.Enabled == true);
            return candidates.FirstOrDefault();
        }
    }

    public static void CreateNewDndClass(DndClass dndClass)
    {
        using (var connection = CreateContext())
        {
            connection.DndClasses.Add(dndClass);    
            SaveChanges(connection);
        }

        Dndcs2.DndLogger.LogInformation($"Class {dndClass.DndClassName} created");
    }
    
    public static dtos.DndSpecie? RetrieveDndSpecie(int dndSpecieId)
    {
        using (var connection = CreateContext())
        {
            var candidates = connection.DndSpecies
                .Where(c => c.DndSpecieId == dndSpecieId && c.Enabled == true);
            return candidates.FirstOrDefault();
        }
    }
    
    public static void CreateNewDndSpecie(dtos.DndSpecie dndSpecie)
    {
        using (var connection = CreateContext())
        {
            connection.DndSpecies.Add(dndSpecie);    
            SaveChanges(connection);
        }

        Dndcs2.DndLogger.LogInformation($"Specie {dndSpecie.DndSpecieName} created");
    }
    
    public static DndPlayer TrackPlayerLogout(DndPlayer dndPlayer, string creator)
    {
        dndPlayer.LastDisconnected = DateTime.UtcNow;
        dndPlayer.UpdatedDate = DateTime.UtcNow;
        dndPlayer.UpdatedBy = creator;
        TimeSpan? sessionTime = dndPlayer.LastDisconnected - dndPlayer.LastConnected;        
        dndPlayer.PlayTime += sessionTime.Value;
        using (var connection = CreateContext())            
            SaveChanges(connection);
        
        return dndPlayer;
    }

    public static void SaveChanges(DndcsContext context)
    {
        context.SaveChangesAsync();
    }
    
}