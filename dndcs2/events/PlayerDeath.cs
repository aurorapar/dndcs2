using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using dndcs2.constants;
using Dndcs2.Sql;

namespace Dndcs2.events;

public class PlayerDeath : DndEvent<EventPlayerDeath>
{
    public Dictionary<int, List<int>> KillTracker = new();

    public PlayerDeath() : base()
    {
        
    }

    public override HookResult DefaultPostHookCallback(EventPlayerDeath @event, GameEventInfo info)
    {
        
        if (@event.Attacker == null || @event.Attacker.UserId == null || @event.Userid == null || @event.Userid.UserId == null)
            return HookResult.Continue;
        
        var victim = @event.Userid;
        var attacker = @event.Attacker;
        
        var dndPlayerVictim = CommonMethods.RetrievePlayer(victim);
        var dndPlayerAttacker = CommonMethods.RetrievePlayer(attacker);

        var victimClassEnum = (constants.DndClass) dndPlayerVictim.DndClassId;
        var victimSpecieEnum = (constants.DndSpecie) dndPlayerVictim.DndSpecieId;
        
        var attackerClassEnum = (constants.DndClass) dndPlayerAttacker.DndClassId;
        var attackerSpecieEnum = (constants.DndSpecie) dndPlayerAttacker.DndSpecieId;

        List<DndClassSpecieEventFeatureContainer> features = new();
        foreach(var classSpecieEventFeature in PostEventCallbacks)
        {
            var feature = (DndClassSpecieEventFeature<EventPlayerDeath>) classSpecieEventFeature; 
            if(
                (feature.DndClass == victimClassEnum
                || feature.DndSpecie == victimSpecieEnum
                || feature.DndClass == attackerClassEnum
                || feature.DndSpecie == attackerSpecieEnum)
                && feature.HookMode == HookMode.Post
            )
                features.Add(feature);
            
        }

        features = features.OrderBy(feature =>((DndClassSpecieEventFeature<EventPlayerDeath>) feature).Priority).ToList();
        bool overrideFlag = false;
        foreach(var f in features)
        {
            var feature = (DndClassSpecieEventFeature<EventPlayerDeath>) f;
            HookResult result = feature.Callback(@event, info);
            if (feature.Priority == DndClassSpecieEventPriority.Interrupts)
                return result;
            if (result != HookResult.Continue)
                return result;
            if(feature.OverrideDefaultBehavior)
                overrideFlag = true;
        };
        
        if(overrideFlag)
            return HookResult.Continue;
        
        if(!KillTracker.ContainsKey((int) attacker.UserId))
            KillTracker[(int) attacker.UserId] = new List<int>();
        KillTracker[(int) attacker.UserId].Add(CommonMethods.RetrievePlayerClassProgress(dndPlayerVictim).DndLevelAmount);
        
        return HookResult.Continue;
    }
}
