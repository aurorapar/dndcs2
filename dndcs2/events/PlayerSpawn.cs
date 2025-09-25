using CounterStrikeSharp.API.Core;
using Dndcs2.dtos;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class PlayerSpawn : DndEvent<EventPlayerSpawn>
{

    public PlayerSpawn() : base()
    {
    
    }

    public override HookResult DefaultPostHookCallback(EventPlayerSpawn @event, GameEventInfo info)
    {         
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");
        
        string message = $"{@event.Userid.PlayerName} spawned, ";
        
        var dndPlayer = CommonMethods.RetrievePlayer(@event.Userid, true);
        if (dndPlayer != null)
            WelcomePlayerBack(dndPlayer, message);
        else
            WelcomeNewPlayer(@event.Userid, message);
        
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