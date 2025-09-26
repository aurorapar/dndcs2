using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class HostageRescued : DndEvent<EventHostageRescued>
{

    public HostageRescued() : base()
    {
        
    }

    public override HookResult DefaultPostHookCallback(EventHostageRescued @event, GameEventInfo info)
    { 
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");        
        
        var dndPlayer = CommonMethods.RetrievePlayer(@event.Userid);        
        GrantPlayerExperience(dndPlayer, Dndcs2.HostageRescuedXP.Value, Dndcs2.HostageRescuedXP.Description, GetType().Name);    
        
        return HookResult.Continue;
    }
}
