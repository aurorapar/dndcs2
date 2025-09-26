using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.Sql;

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

        var dndPlayer = CommonMethods.RetrievePlayer(@event.Userid, true);
        if (@event.Userid.IsBot)
        {
            if (dndPlayer != null)
                CommonMethods.TrackPlayerLogin(dndPlayer, DateTime.UtcNow, GetType().Name);
            else
                CommonMethods.CreateNewPlayer(@event.Userid, GetType().Name);
        }

        if(dndPlayer == null)
            dndPlayer = CommonMethods.RetrievePlayer(@event.Userid);
        
        RoundStart roundStartEvent = (RoundStart) DndEvent<EventRoundStart>.RetrieveEvent<EventRoundStart>();
        if(!roundStartEvent.XpRoundTracker.ContainsKey(dndPlayer.DndPlayerId))
            roundStartEvent.XpRoundTracker[dndPlayer.DndPlayerId] = new List<DndExperienceLog>();

        PlayerDeath playerDeathEvent = (PlayerDeath) DndEvent<EventPlayerDeath>.RetrieveEvent<EventPlayerDeath>();
        if (!playerDeathEvent.KillStreakTracker.ContainsKey(@event.Userid))
            playerDeathEvent.KillStreakTracker[@event.Userid] = 0;
        
        Dndcs2.ShowDndXp(@event.Userid, @event.Userid);
        if(dndPlayer.PlayTimeHours < 5)
            MessagePlayer(@event.Userid, "Command for the mod are 'dndinfo', 'dndmenu', and 'dndxp'");
        
        return HookResult.Continue;   
    }    
}