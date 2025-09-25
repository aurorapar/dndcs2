using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
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

        var accountId = new SteamID(@event.Userid.SteamID).AccountId;
        var dndPlayer = CommonMethods.RetrievePlayer(accountId, true);
        if (dndPlayer != null)
            WelcomePlayerBack(dndPlayer, message);
        else
            WelcomeNewPlayer(accountId, message);

        return HookResult.Continue;
    }

    private DndPlayer WelcomePlayerBack(DndPlayer dndPlayer, string message)
    {
        CommonMethods.TrackPlayerLogin(dndPlayer, DateTime.UtcNow, GetType().Name);
        message += "welcome them back!";
        BroadcastMessage(message);
        return dndPlayer;
    }

    private DndPlayer WelcomeNewPlayer(int accountId, string message)
    {
        var dndPlayer = CommonMethods.CreateNewPlayer(accountId, GetType().Name);
        message += "help out the new player!";
        BroadcastMessage(message);
        return dndPlayer;
    }
}