using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.Sql;
using Dndcs2.stats;

namespace Dndcs2.events;

public class PlayerSpawn : DndEvent<EventPlayerSpawn>
{

    public PlayerSpawn() : base()
    {
        
    }
    
    public override HookResult DefaultPreHookCallback(EventPlayerSpawn @event, GameEventInfo info)
    {         
        if (@event.Userid == null)
            return HookResult.Continue;
        if(@event.Userid.ControllingBot || (int) @event.Userid.Team == 0)      
            return HookResult.Continue;

        if (@event.Userid.IsBot)
        {
            if(CommonMethods.RetrievePlayer(@event.Userid) == null)
                CommonMethods.CreateNewPlayer(@event.Userid, GetType().Name);
        }
        
        var player = @event.Userid;
        var playerStats = PlayerStats.GetPlayerStats(player);
        playerStats.Reset();
        return HookResult.Continue;   
    }

    public override HookResult DefaultPostHookCallback(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (@event.Userid == null)
            return HookResult.Continue;
        if(@event.Userid.ControllingBot || (int) @event.Userid.Team == 0)      
            return HookResult.Continue;

        var player = @event.Userid;
        var dndPlayer = CommonMethods.RetrievePlayer(player);
        var dndClass = (constants.DndClass) dndPlayer.DndClassId;
        var specie = (constants.DndSpecie) dndPlayer.DndSpecieId;
        
        List<EventCallbackFeatureContainer> features = new();
        foreach(var classSpecieEventFeature in PostEventCallbacks)
        {
            var feature = (EventCallbackFeature<EventPlayerSpawn>) classSpecieEventFeature; 
            if(
                (feature.DndClass == dndClass || feature.DndSpecie == specie || (feature.DndClass == null && feature.DndSpecie == null))                                             
                && feature.HookMode == HookMode.Post
            )
                features.Add(feature);            
        }

        features = features.OrderBy(feature =>((EventCallbackFeature<EventPlayerSpawn>) feature).CallbackFeaturePriority).ToList();
        bool overrideFlag = false;
        foreach(var f in features)
        {
            var feature = (EventCallbackFeature<EventPlayerSpawn>) f;
            HookResult result = feature.Callback(@event, info, dndPlayer, null);
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