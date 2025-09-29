using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
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
        
        var playerStats = PlayerStats.GetPlayerStats((int) @event.Userid.UserId);
        playerStats.Reset();        

        var spawnerClassEnum = (constants.DndClass)dndPlayer.DndClassId;
        var spawnerSpecieEnum = (constants.DndSpecie)dndPlayer.DndSpecieId;
        List<EventCallbackFeatureContainer> features = new();
        foreach(var classSpecieEventFeature in PreEventCallbacks)
        {
            var feature = (EventCallbackFeature<EventPlayerSpawn>) classSpecieEventFeature; 
            if(
                (feature.DndClass == spawnerClassEnum
                 || feature.DndSpecie == spawnerSpecieEnum
                 || (feature.DndClass == null && feature.DndSpecie == null) // used for event for all classes & species
                 ) && feature.HookMode == HookMode.Pre
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

    public override HookResult DefaultPostHookCallback(EventPlayerSpawn @event, GameEventInfo info)
    {         
        if (@event.Userid == null || @event.Userid.PawnIsAlive != true)
            return HookResult.Continue;

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
        
        Dndcs2.InitializeTrackers(@event.Userid, dndPlayer);
        
        Dndcs2.ShowDndXp(@event.Userid, @event.Userid);
        if(dndPlayer.PlayTimeHours < 5)
            MessagePlayer(@event.Userid, "Command for the mod are 'dndinfo', 'dndmenu', and 'dndxp'");

        var spawnerClassEnum = (constants.DndClass)dndPlayer.DndClassId;
        var spawnerSpecieEnum = (constants.DndSpecie)dndPlayer.DndSpecieId;
        List<EventCallbackFeatureContainer> features = new();
        foreach(var classSpecieEventFeature in PostEventCallbacks)
        {
            var feature = (EventCallbackFeature<EventPlayerSpawn>) classSpecieEventFeature; 
            if(
                (feature.DndClass == spawnerClassEnum
                 || feature.DndSpecie == spawnerSpecieEnum
                 || (feature.DndClass == null && feature.DndSpecie == null) // used for event for all classes & species
                 ) && feature.HookMode == HookMode.Post
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