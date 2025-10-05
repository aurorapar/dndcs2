using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using static Dndcs2.messages.DndMessages;
using Dndcs2.constants;
using Dndcs2.Sql;
using Dndcs2.stats;
using Dndcs2.timers;

namespace Dndcs2.events;

public class PlayerHurt : DndEvent<EventPlayerHurt>
{

    public PlayerHurt() : base()
    {
        
    }

    public override HookResult DefaultPreHookCallback(EventPlayerHurt @event, GameEventInfo info)
    {

        var attacker = @event.Attacker;
        var victim = @event.Userid;
        if (attacker == null || victim == null)            
            return HookResult.Continue;

        var dndPlayerVictim = CommonMethods.RetrievePlayer(victim);
        var dndPlayerAttacker = CommonMethods.RetrievePlayer(attacker);

        var victimClassEnum = (constants.DndClass) dndPlayerVictim.DndClassId;
        var victimSpecieEnum = (constants.DndSpecie) dndPlayerVictim.DndSpecieId;

        var attackerClassEnum = (constants.DndClass) dndPlayerAttacker.DndClassId;
        var attackerSpecieEnum = (constants.DndSpecie) dndPlayerAttacker.DndSpecieId;

        var victimStats = PlayerStats.GetPlayerStats(victim);
        victimStats.ChangeSpeed(0, .1f);
        
        if(attacker.IsBot)
            Dndcs2.DamageTarget(attacker, attacker, 100);

        List<EventCallbackFeatureContainer> features = new();
        foreach(var classSpecieEventFeature in PreEventCallbacks)
        {
            var feature = (EventCallbackFeature<EventPlayerHurt>) classSpecieEventFeature; 
            if(
                (feature.DndClass == victimClassEnum
                 || feature.DndSpecie == victimSpecieEnum
                 || (feature.DndClass == attackerClassEnum || feature.DndSpecie == attackerSpecieEnum) && ! attacker.ControllingBot)
                && feature.HookMode == HookMode.Pre
            )
                features.Add(feature);
            
        }

        features = features.OrderBy(feature =>((EventCallbackFeature<EventPlayerHurt>) feature).CallbackFeaturePriority).ToList();
        bool overrideFlag = false;
        foreach(var f in features)
        {
            var feature = (EventCallbackFeature<EventPlayerHurt>) f;
            HookResult result = feature.Callback(@event, info, dndPlayerVictim, dndPlayerAttacker);
            if (feature.CallbackFeaturePriority == EventCallbackFeaturePriority.Interrupts)
                return result;
            if (result != HookResult.Continue)
                return result;
            if(feature.OverrideDefaultBehavior)
                overrideFlag = true;
        };
        
        if(overrideFlag)
            return HookResult.Continue;        
        
        return HookResult.Continue;
    }
    
    public override HookResult DefaultPostHookCallback(EventPlayerHurt @event, GameEventInfo info)
    {

        var attacker = @event.Attacker;
        var victim = @event.Userid;
        if (attacker == null || victim == null)            
            return HookResult.Continue;

        var dndPlayerVictim = CommonMethods.RetrievePlayer(victim);
        var dndPlayerAttacker = CommonMethods.RetrievePlayer(attacker);

        var victimClassEnum = (constants.DndClass) dndPlayerVictim.DndClassId;
        var victimSpecieEnum = (constants.DndSpecie) dndPlayerVictim.DndSpecieId;
        
        var attackerClassEnum = (constants.DndClass) dndPlayerAttacker.DndClassId;
        var attackerSpecieEnum = (constants.DndSpecie) dndPlayerAttacker.DndSpecieId;

        List<EventCallbackFeatureContainer> features = new();
        foreach(var classSpecieEventFeature in PostEventCallbacks)
        {
            var feature = (EventCallbackFeature<EventPlayerHurt>) classSpecieEventFeature; 
            if(
                (feature.DndClass == victimClassEnum
                 || feature.DndSpecie == victimSpecieEnum
                 || (feature.DndClass == attackerClassEnum || feature.DndSpecie == attackerSpecieEnum) && ! attacker.ControllingBot)
                && feature.HookMode == HookMode.Post
            )
                features.Add(feature);
            
        }

        features = features.OrderBy(feature =>((EventCallbackFeature<EventPlayerHurt>) feature).CallbackFeaturePriority).ToList();
        bool overrideFlag = false;
        foreach(var f in features)
        {
            var feature = (EventCallbackFeature<EventPlayerHurt>) f;
            HookResult result = feature.Callback(@event, info, dndPlayerVictim, dndPlayerAttacker);
            if (feature.CallbackFeaturePriority == EventCallbackFeaturePriority.Interrupts)
                return result;
            if (result != HookResult.Continue)
                return result;
            if(feature.OverrideDefaultBehavior)
                overrideFlag = true;
        };
        
        if(overrideFlag)
            return HookResult.Continue;        
        
        return HookResult.Continue;
    }
}
