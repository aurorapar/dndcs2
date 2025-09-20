using Dndcs2.constants;
using Dndcs2.dtos;
using Microsoft.Extensions.Logging;

namespace Dndcs2.Sql;

public static class CommonMethods
{
    public static DndcsContext CreateContext()
    {
        return new DndcsContext();
    }
    
    public static DndPlayer? RetrievePlayer(int accountId)
    {
        using (var connection = CreateContext())
        {
            var candidates = connection.DndPlayers
                .Where(p => p.DndPlayerId == accountId && p.Enabled == true);
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
            (int)DndClasses.Fighter,
            (int)DndSpecies.Human,
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