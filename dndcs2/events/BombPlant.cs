using CounterStrikeSharp.API.Core;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class BombPlant : DndEvent<EventBombPlanted>
{

    public BombPlant() : base()
    {
        
    }

    public override HookResult DefaultPostHookCallback(EventBombPlanted @event, GameEventInfo info)
    { 
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");        
        
        var dndPlayer = CommonMethods.RetrievePlayer(@event.Userid); 
        GrantPlayerExperience(dndPlayer, Dndcs2.BombPlantedXP.Value, Dndcs2.BombPlantedXP.Description, GetType().Name);
        
        return HookResult.Continue;
    }
}
