using CounterStrikeSharp.API.Core;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class BombExplode : DndEvent<EventBombExploded>
{

    public BombExplode() : base()
    {
        
    }

    public override HookResult DefaultPostHookCallback(EventBombExploded @event, GameEventInfo info)
    { 
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");        
        
        var dndPlayer = CommonMethods.RetrievePlayer(@event.Userid); 
        GrantPlayerExperience(dndPlayer, Dndcs2.BombExplodedXP.Value, Dndcs2.BombExplodedXP.Description, GetType().Name);
        
        return HookResult.Continue;
    }
}
