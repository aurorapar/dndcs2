using CounterStrikeSharp.API.Core;
using Dndcs2.dtos;
using static Dndcs2.messages.DndMessages;
using Dndcs2.Sql;

namespace Dndcs2.events;

public class PlayerConnectFull: DndEvent<EventPlayerConnectFull>
{
    public Dictionary<int, List<int>> KillTracker = new();

    public PlayerConnectFull() : base()
    {
        
    }

    public override HookResult DefaultPostHookCallback(EventPlayerConnectFull @event, GameEventInfo info)
    { 
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");
     
        string message = $"{@event.Userid.PlayerName} connected, ";
        
        var dndPlayer = CommonMethods.RetrievePlayer(@event.Userid, true);
        if (dndPlayer != null)
            WelcomePlayerBack(dndPlayer, message);
        else
            WelcomeNewPlayer(@event.Userid, message);
        
        dndPlayer = CommonMethods.RetrievePlayer(@event.Userid, true);
        var playTime = dndPlayer.PlayTimeHours + (DateTime.UtcNow - dndPlayer.LastConnected).TotalHours; 
        MessagePlayer(@event.Userid, $"Your total playtime: {(int) playTime} hours {(int)((playTime - (int) playTime)*60)} minutes");
        return HookResult.Continue;
    }

    private DndPlayer WelcomePlayerBack(DndPlayer dndPlayer, string message)
    {
        CommonMethods.TrackPlayerLogin(dndPlayer, DateTime.UtcNow, GetType().Name);
        message += "welcome them back!";
        BroadcastMessage(message);
        return dndPlayer;
    }

    private DndPlayer WelcomeNewPlayer(CCSPlayerController player, string message)
    {
        var dndPlayer = CommonMethods.CreateNewPlayer(player, GetType().Name);
        message += "help out the new player!";
        BroadcastMessage(message);
        return dndPlayer;
    }
}