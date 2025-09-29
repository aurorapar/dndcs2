using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static Dndcs2.messages.DndMessages;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.Sql;

namespace Dndcs2.events;

public class RoundEnd : DndEvent<EventRoundEnd>
{
    public RoundEnd() : base()
    {
        
    }

    public override HookResult DefaultPostHookCallback(EventRoundEnd @event, GameEventInfo info)
    {
        foreach (var player in Utilities.GetPlayers())
        {
            var dndPlayer = CommonMethods.RetrievePlayer(player);
            var dndClassEnum = (constants.DndClass) dndPlayer.DndClassId;
            var dndSpecieEnum = (constants.DndSpecie) dndPlayer.DndSpecieId;
            
            List<EventCallbackFeatureContainer> features = new();
            foreach(var classSpecieEventFeature in PostEventCallbacks)
            {
                var feature = (EventCallbackFeature<EventRoundEnd>) classSpecieEventFeature; 
                if(
                    (feature.DndClass == dndClassEnum
                     || feature.DndSpecie == dndSpecieEnum
                    )
                    && feature.HookMode == HookMode.Post
                )
                    features.Add(feature);            
            }
            
            features = features.OrderBy(feature =>((EventCallbackFeature<EventRoundEnd>) feature).CallbackFeaturePriority).ToList();
            bool overrideFlag = false;
            if(!features.Any())
                DoDefaultPostCallback(@event.Winner, player, dndPlayer);
            foreach(var f in features)
            {
                var feature = (EventCallbackFeature<EventRoundEnd>) f;
                HookResult result = feature.Callback(@event, info, dndPlayer, null);
                if (feature.CallbackFeaturePriority == EventCallbackFeaturePriority.Interrupts)
                    break;
                if (result != HookResult.Continue)
                    break;
                if(!feature.OverrideDefaultBehavior)
                    DoDefaultPostCallback(@event.Winner, player, dndPlayer);
            };
        }        
        
        return HookResult.Continue;
    }

    public void DoDefaultPostCallback(int winner, CCSPlayerController player, DndPlayer dndPlayer)
    {        
        if(winner == (int) player.Team)
            GrantPlayerExperience(dndPlayer, Dndcs2.RoundWonXP.Value, Dndcs2.RoundWonXP.Description, GetType().Name);

        Dndcs2.ProcessPlayerXp(player);
    }
}
