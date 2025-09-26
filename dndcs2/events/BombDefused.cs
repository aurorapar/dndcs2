using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class BombDefused : DndEvent<EventBombDefused>
{

    public BombDefused() : base()
    {
        
    }

    public override HookResult DefaultPostHookCallback(EventBombDefused @event, GameEventInfo info)
    { 
        if (@event.Userid == null)
            throw new Exception($"{GetType().Name} Userid was null");        
        
        var dndPlayer = CommonMethods.RetrievePlayer(@event.Userid); 
        
        if(!Utilities.GetPlayers().Where(p => p.Team != @event.Userid.Team && p.PawnIsAlive).Any())
            GrantPlayerExperience(dndPlayer, Dndcs2.BombDefusedXP.Value, Dndcs2.BombDefusedXP.Description, GetType().Name);
        else 
            GrantPlayerExperience(dndPlayer, Dndcs2.BombDefusedWithEnemiesXP.Value, Dndcs2.BombDefusedWithEnemiesXP.Description, GetType().Name);
        
        return HookResult.Continue;
    }
}
