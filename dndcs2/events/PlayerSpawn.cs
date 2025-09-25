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
        
        var playTime = dndPlayer.PlayTimeHours + (DateTime.UtcNow - dndPlayer.LastConnected).TotalHours; 
        MessagePlayer(@event.Userid, $"Your total playtime: {(int) playTime} hours {(int)((playTime - (int) playTime)*60)} minutes");
        
        return HookResult.Continue;   
    }    
}