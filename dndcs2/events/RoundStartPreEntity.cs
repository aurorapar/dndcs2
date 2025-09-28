using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.Sql;
using Dndcs2.stats;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class RoundStartPreEntity : DndEvent<EventRoundStartPreEntity>
{    
    public static Dictionary<int, PlayerBaseStats>? PlayerStats = null;
    
    public RoundStartPreEntity() : base()
    {
        PrintMessageToConsole("Registering RoundStartPreEntity event listener");
    }

    public override HookResult DefaultPostHookCallback(EventRoundStartPreEntity @event, GameEventInfo info)
    {        
        PrintMessageToConsole("Starting RoundStartPreEntity event listener");
        if (PlayerStats == null)
            PlayerStats = new();        
        
        foreach (var player in Utilities.GetPlayers())
        {               
            var dndPlayer = CommonMethods.RetrievePlayer(player);
            var dndClassEnum = (constants.DndClass) dndPlayer.DndClassId;
            var dndSpecieEnum = (constants.DndSpecie) dndPlayer.DndSpecieId;
            
            List<EventCallbackFeatureContainer> features = new();
            foreach(var classSpecieEventFeature in PostEventCallbacks)
            {
                var feature = (EventCallbackFeature<EventRoundStartPreEntity>) classSpecieEventFeature; 
                if(
                    (feature.DndClass == dndClassEnum
                     || feature.DndSpecie == dndSpecieEnum
                     || (feature.DndClass == null && feature.DndSpecie == null)
                    )
                    && feature.HookMode == HookMode.Post
                )
                    features.Add(feature);            
            }
            
            features = features.OrderBy(feature =>((EventCallbackFeature<EventRoundStartPreEntity>) feature).CallbackFeaturePriority).ToList();
            bool overrideFlag = false;
            foreach(var f in features)
            {
                var feature = (EventCallbackFeature<EventRoundStartPreEntity>) f;
                HookResult result = feature.Callback(@event, info, dndPlayer, null);
                if (feature.CallbackFeaturePriority == EventCallbackFeaturePriority.Interrupts)
                    break;
                if (result != HookResult.Continue)
                    break;
                if(!feature.OverrideDefaultBehavior)
                    DoDefaultPostCallback(@event, info, dndPlayer);
            };
        }
        
        return HookResult.Continue;
    }

    public void DoDefaultPostCallback(EventRoundStartPreEntity @event, GameEventInfo info, DndPlayer dndPlayer)
    {
        PrintMessageToConsole("Doing RoundStartPreEntity default behavior");
    }
}
