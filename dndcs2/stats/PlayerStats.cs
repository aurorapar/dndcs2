using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.dtos;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;
namespace Dndcs2.stats;

public static class PlayerStats
{
    private static Dictionary<int, PlayerBaseStats> _playerStatsLookup = new();

    public static PlayerBaseStats? GetPlayerStats(DndPlayer dndPlayer)
    {
        var player = Utilities.GetPlayers()
            .FirstOrDefault(p => CommonMethods.GetPlayerAccountId(p) == dndPlayer.DndPlayerAccountId);

        if (player == null)
            return null;


        if (!_playerStatsLookup.ContainsKey((int)player.UserId))
            _playerStatsLookup[(int)player.UserId] = new PlayerBaseStats((int) player.UserId);
        return _playerStatsLookup[(int)player.UserId];
    }
    
    public static PlayerBaseStats? GetPlayerStats(int userid)
    {        
        if (!_playerStatsLookup.ContainsKey(userid))
            _playerStatsLookup[userid] = new PlayerBaseStats(userid);
        return _playerStatsLookup[userid];
    }
    
    public static PlayerBaseStats? GetPlayerStats(CCSPlayerController player)
    {           
        return GetPlayerStats((int) player.UserId);
    }
}
