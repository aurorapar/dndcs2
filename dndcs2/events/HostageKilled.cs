using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class HostageKilled : DndEvent<EventHostageKilled>
{

    public HostageKilled() : base()
    {
        
    }

    public override HookResult DefaultPostHookCallback(EventHostageKilled @event, GameEventInfo info)
    { 
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");        
        
        var dndPlayer = CommonMethods.RetrievePlayer(@event.Userid);        
        GrantPlayerExperience(dndPlayer, Dndcs2.HostageKilledXP.Value, Dndcs2.HostageKilledXP.Description, GetType().Name);    
        
        return HookResult.Continue;
    }
}
