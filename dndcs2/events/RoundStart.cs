using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.Sql;

namespace Dndcs2.events;

public class RoundStart : DndEvent<EventRoundStart>
{
    public Dictionary<int, List<DndExperienceLog>> XpRoundTracker { get; private set; } = new();
    
    public RoundStart() : base()
    {
        
    }

    public override HookResult DefaultPostHookCallback(EventRoundStart @event, GameEventInfo info)
    {
        foreach (var player in Utilities.GetPlayers())
        {            
            var dndPlayer = CommonMethods.RetrievePlayer(player);
            var dndClassEnum = (constants.DndClass) dndPlayer.DndClassId;
            var dndSpecieEnum = (constants.DndSpecie) dndPlayer.DndSpecieId;
            
            XpRoundTracker[dndPlayer.DndPlayerId] = new List<DndExperienceLog>();
            
            List<DndClassSpecieEventFeatureContainer> features = new();
            foreach(var classSpecieEventFeature in PostEventCallbacks)
            {
                var feature = (DndClassSpecieEventFeature<EventRoundStart>) classSpecieEventFeature; 
                if(
                    (feature.DndClass == dndClassEnum
                     || feature.DndSpecie == dndSpecieEnum
                    )
                    && feature.HookMode == HookMode.Post
                )
                    features.Add(feature);            
            }
            
            features = features.OrderBy(feature =>((DndClassSpecieEventFeature<EventRoundStart>) feature).Priority).ToList();
            bool overrideFlag = false;
            foreach(var f in features)
            {
                var feature = (DndClassSpecieEventFeature<EventRoundStart>) f;
                HookResult result = feature.Callback(@event, info);
                if (feature.Priority == DndClassSpecieEventPriority.Interrupts)
                    break;
                if (result != HookResult.Continue)
                    break;
                if(!feature.OverrideDefaultBehavior)
                    DoDefaultPostCallback(@event, info, dndPlayer);
            };
        }        
        
        return HookResult.Continue;
    }

    public void DoDefaultPostCallback(EventRoundStart @event, GameEventInfo info, DndPlayer dndPlayer)
    {
        
    }
}
