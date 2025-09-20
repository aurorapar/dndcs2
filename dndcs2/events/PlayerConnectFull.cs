using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using Dndcs2.dtos;
using dndcs2.events;
using static Dndcs2.messages.DndMessages;
using Dndcs2.Sql;

namespace Dndcs2.events;

public class PlayerConnectFull<T, TU, TV> : DndBaseEvent<T, TU, TV>
    where T : GameEvent
    where TU : GameEvent
    where TV : GameEvent

{    
    public override HookResult PostHookCallback(TU e, GameEventInfo info)
    {
        var @event = ConvertEventType<EventPlayerConnectFull>(e);  
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");

        string message = $"{@event.Userid.PlayerName} connected, ";

        var accountId = new SteamID(@event.Userid.SteamID).AccountId;
        var dndPlayer = CommonMethods.RetrievePlayer(accountId);
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