using System.Runtime.CompilerServices;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DndClass = Dndcs2.dtos.DndClass;

namespace Dndcs2.Sql;

public static class CommonMethods
{
    public static DndcsContext CreateContext()
    {
        return new DndcsContext();
    }

    public static int GetPlayerAccountId(CCSPlayerController player)
    {
        return player.IsBot ? player.PlayerName.Length : new SteamID(player.SteamID).AccountId;
    }
    
    public static DndPlayer? RetrievePlayer(CCSPlayerController player, bool atConnectionTime)
    {
        // Players should *always* be created at connection. If that does not occur, I WANT any call to this method to
        // fail hard, not gracefully. Ignore any warnings caused at any time OTHER than testing for null values at 
        // creation
        int accountId = GetPlayerAccountId(player);
        using (var connection = CreateContext())
        {
            var candidates = connection.DndPlayers
                .Where(p => p.DndPlayerAccountId == accountId && p.Enabled == true);
            
            return candidates.FirstOrDefault();            
        }
    }
    
    public static DndPlayer RetrievePlayer(CCSPlayerController player)
    {
        // Players should *always* be created at connection. If that does not occur, I WANT any call to this method to
        // fail hard, not gracefully. Ignore any warnings caused at any time OTHER than testing for null values at 
        // creation
        int accountId = GetPlayerAccountId(player);
        using (var connection = CreateContext())
        {
            var candidates = connection.DndPlayers
                .Where(p => p.DndPlayerAccountId == accountId && p.Enabled == true);
            
            return candidates.First();
        }
    }

    public static DndSpecieProgress RetrievePlayerSpecieProgress(CCSPlayerController player)
    {
        DndPlayer dndPlayer =  RetrievePlayer(player);
         
        using (var connection = CreateContext())
        {
            var candidates = connection.DndSpecieProgresses
                .Where(s => s.DndPlayerId == dndPlayer.DndPlayerId && 
                            s.DndSpecieId == dndPlayer.DndSpecieId && s.Enabled == true);
            return candidates.First();
        }
    }
    
    public static DndClassProgress RetrievePlayerClassProgress(CCSPlayerController player)
    {
        DndPlayer dndPlayer =  RetrievePlayer(player);        
         
        using (var connection = CreateContext())
        {
            var candidates = connection.DndClassProgresses
                .Where(s => s.DndPlayerId == dndPlayer.DndPlayerId && s.DndClassId == dndPlayer.DndClassId &&
                            s.Enabled == true);
            return candidates.First();
        }
    }
    
    public static DndClassProgress? RetrievePlayerClassProgress(CCSPlayerController player, constants.DndClass dndClass)
    {
        DndPlayer dndPlayer =  RetrievePlayer(player);        
         
        using (var connection = CreateContext())
        {
            var candidates = connection.DndClassProgresses
                .Where(s => s.DndPlayerId == dndPlayer.DndPlayerId && s.DndClassId == (int) dndClass 
                    && s.Enabled == true);
            return candidates.FirstOrDefault();
        }
    }
    
    public static DndSpecieProgress? RetrievePlayerSpecieProgress(CCSPlayerController player, constants.DndSpecie dndSpecie)
    {
        DndPlayer dndPlayer =  RetrievePlayer(player);        
         
        using (var connection = CreateContext())
        {
            var candidates = connection.DndSpecieProgresses
                .Where(s => s.DndPlayerId == dndPlayer.DndPlayerId && s.DndSpecieId == (int) dndSpecie 
                                                                   && s.Enabled == true);
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
    
    public static DndPlayer CreateNewPlayer(CCSPlayerController player, string creator)
    {
        var dndPlayer = new DndPlayer(
            creator,
            DateTime.UtcNow,
            creator,
            DateTime.UtcNow,
            true,
            player.IsBot ? player.PlayerName.Length : new SteamID(player.SteamID).AccountId,
            // TODO: Random starting gold? Setting?
            0,
            (int)constants.DndClass.Fighter,
            (int)constants.DndSpecie.Human,
            DateTime.UtcNow
        );
        
        using (var connection = CreateContext())
        {
            connection.DndPlayers.Add(dndPlayer);
            connection.Entry(dndPlayer).State = EntityState.Added;
            SaveChanges(connection);            
        }
        
        CreateNewClassProgress(player, constants.DndClass.Fighter, creator);
        CreateNewSpecieProgress(player, constants.DndSpecie.Human, creator);
        Dndcs2.Instance.Log.LogInformation($"Player {dndPlayer.DndPlayerAccountId} created");
        return dndPlayer;
    }

    public static void CreateNewClassProgress(CCSPlayerController player, constants.DndClass dndClass, string creator)
    {
        var dndPlayer = RetrievePlayer(player);
        var classProgress = new DndClassProgress(
            creator,
            DateTime.UtcNow,
            creator,
            DateTime.UtcNow,
            true,
            dndPlayer.DndPlayerId,
            (int) dndClass
        );

        using (var connection = CreateContext())
        {
            connection.DndClassProgresses.Add(classProgress);
            connection.Entry(classProgress).State = EntityState.Added;
            SaveChanges(connection);
        }
    }
    
    public static void CreateNewSpecieProgress(CCSPlayerController player, constants.DndSpecie dndSpecie, string creator)
    {
        var dndPlayer = RetrievePlayer(player);
        var specieProgress = new DndSpecieProgress(
            creator,
            DateTime.UtcNow,
            creator,
            DateTime.UtcNow,
            true,
            dndPlayer.DndPlayerId,
            (int) dndSpecie
        );

        using (var connection = CreateContext())
        {
            connection.DndSpecieProgresses.Add(specieProgress);
            connection.Entry(specieProgress).State = EntityState.Added;
            SaveChanges(connection);
        }
    }

    public static DndPlayer TrackPlayerLogin(DndPlayer dndPlayer, DateTime loginTime, string creator)
    {
        dndPlayer.LastConnected = loginTime;
        dndPlayer.UpdatedDate = loginTime;
        dndPlayer.UpdatedBy = creator;
        using (var connection = CreateContext())
        {
            connection.Entry(dndPlayer).State = EntityState.Modified;
            SaveChanges(connection);
        }

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
            connection.Entry(dndClass).State = EntityState.Added;
            SaveChanges(connection);
        }

        // Dndcs2.DndLogger.LogInformation($"Class {dndClass.DndClassName} created");
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

        // Dndcs2.DndLogger.LogInformation($"Specie {dndSpecie.DndSpecieName} created");
    }
    
    public static DndPlayer TrackPlayerLogout(DndPlayer dndPlayer, string creator)
    {
        dndPlayer.LastDisconnected = DateTime.UtcNow;
        dndPlayer.UpdatedDate = DateTime.UtcNow;
        dndPlayer.UpdatedBy = creator;
        TimeSpan sessionTime = (DateTime) dndPlayer.LastDisconnected - dndPlayer.LastConnected;        
        dndPlayer.PlayTimeHours += sessionTime.TotalHours;
        using (var connection = CreateContext())
        {
            connection.Entry(dndPlayer).State = EntityState.Modified;
            SaveChanges(connection);
        }
        return dndPlayer;
    }

    public static void SaveChanges(DndcsContext context)
    {
        context.SaveChangesAsync();
    }

    public static void GrantExperience(CCSPlayerController player, DndExperienceLog xpLogItem)
    {
        var dndClassProgress = RetrievePlayerClassProgress(player);
        var dndSpecieProgress = RetrievePlayerSpecieProgress(player);
        
        dndClassProgress.DndExperienceAmount += xpLogItem.ExperienceAmount;
        dndClassProgress.UpdatedDate = xpLogItem.CreateDate;
        dndClassProgress.UpdatedBy = xpLogItem.CreatedBy;
        
        dndSpecieProgress.DndExperienceAmount += xpLogItem.ExperienceAmount;
        dndSpecieProgress.UpdatedDate = xpLogItem.CreateDate;
        dndSpecieProgress.UpdatedBy = xpLogItem.CreatedBy;

        if (dndClassProgress.DndExperienceAmount >= dndClassProgress.DndLevelAmount * 1000)
        {
            BroadcastMessage($"Congratulations, {player.PlayerName}! You leveled up your class!");
            dndClassProgress.DndExperienceAmount -= dndClassProgress.DndLevelAmount * 1000;
            dndClassProgress.DndLevelAmount += 1;
        }
        if (dndSpecieProgress.DndExperienceAmount >= dndSpecieProgress.DndLevelAmount * 1000 )
        {
            BroadcastMessage($"Congratulations, {player.PlayerName}! You leveled up your specie!");
            dndSpecieProgress.DndExperienceAmount -= dndSpecieProgress.DndLevelAmount * 1000;
            dndSpecieProgress.DndLevelAmount += 1;
        }        

        using (var connection = CreateContext())
        {
            connection.DndExperienceLogs.Add(xpLogItem);
            connection.Entry(dndClassProgress).State = EntityState.Modified;
            connection.Entry(dndSpecieProgress).State = EntityState.Modified;
            connection.Entry(xpLogItem).State = EntityState.Added;
            SaveChanges(connection);
        }
            
    }

    public static List<string> RetrieveDndXpLogs(CCSPlayerController player, int amount=10)
    {
        var dndPlayer = RetrievePlayer(player);
        var logs = new List<string>();
        using (var connection = CreateContext())
        {
            connection.DndExperienceLogs
                .Where(l => l.DndPlayerId == dndPlayer.DndPlayerId)
                .OrderByDescending(l => l.CreateDate)
                .Take(amount).ToList()
                .ForEach(l => logs.Add($"{l.ExperienceLogId} {l.Reason} {l.ExperienceAmount} {l.CreateDate} {l.UpdatedDate}"));
            
        }
        return logs;
    }

    public static int RetrievePlayerClassLevel(CCSPlayerController player)
    {
        return RetrievePlayerClassProgress(player).DndLevelAmount;
    }
    
    public static int RetrievePlayerSpecieLevel(CCSPlayerController player)
    {
        return RetrievePlayerSpecieProgress(player).DndLevelAmount;
    }

    public static bool ChangeClass(CCSPlayerController player, constants.DndClass newClass)
    {
        if (player.PawnIsAlive)
            return false;
        
        var dndPlayer = RetrievePlayer(player);
        if (dndPlayer.DndClassId == (int)newClass)
            return false;
        
        if (!CanPlayClass(player, newClass))
            return false;
        
        var progress = RetrievePlayerClassProgress(player, newClass);
        if (progress == null)
            CreateNewClassProgress(player, newClass, "ChangeClass");
        
        dndPlayer.DndClassId = (int)newClass;
        dndPlayer.UpdatedDate = DateTime.Now;
        dndPlayer.UpdatedBy = "ChangeClass";
        using (var connection = CreateContext())
        {
            connection.Entry(dndPlayer).State = EntityState.Modified;
            SaveChanges(connection);
        }

        return true;
    }

    public static bool CanPlayClass(CCSPlayerController player, constants.DndClass dndClass)
    {
        var dndClassRequirements = Dndcs2.Instance.DndClassLookup[dndClass].DndClassRequirements;
        
        if (!dndClassRequirements.Any())
            return true;
        foreach (var requirement in dndClassRequirements)
        {
            var progress = RetrievePlayerClassProgress(player, (constants.DndClass) requirement.DndRequiredClassId);
            if (progress == null)
                return false;
            if (progress.DndLevelAmount < requirement.DndRequiredClassLevel)
                return false;
        }
        return true;
    }
    
    public static bool ChangeSpecie(CCSPlayerController player, constants.DndSpecie newSpecie)
    {
        if (player.PawnIsAlive)
            return false;
        
        var dndPlayer = RetrievePlayer(player);
        if (dndPlayer.DndSpecieId == (int)newSpecie)
            return false;
        
        if (!CanPlaySpecie(player, newSpecie))
            return false;
        
        var progress = RetrievePlayerSpecieProgress(player, newSpecie);
        if (progress == null)
            CreateNewSpecieProgress(player, newSpecie, "ChangeSpecie");
        
        dndPlayer.DndSpecieId = (int)newSpecie;
        dndPlayer.UpdatedDate = DateTime.Now;
        dndPlayer.UpdatedBy = "ChangeSpecie";
        using (var connection = CreateContext())
        {
            connection.Entry(dndPlayer).State = EntityState.Modified;
            SaveChanges(connection);
        }

        return true;
    }
    
    public static bool CanPlaySpecie(CCSPlayerController player, constants.DndSpecie dndSpecie)
    {
        var dndSpecieRequirements = Dndcs2.Instance.DndSpecieLookup[dndSpecie].DndSpecieRequirements;
        
        if (!dndSpecieRequirements.Any())
            return true;
        foreach (var requirement in dndSpecieRequirements)
        {
            var progress = RetrievePlayerSpecieProgress(player, (constants.DndSpecie) requirement.DndRequiredSpecieLevel);
            if (progress == null)
                return false;
            if (progress.DndLevelAmount < requirement.DndRequiredSpecieLevel)
                return false;
        }
        return true;
    }
}