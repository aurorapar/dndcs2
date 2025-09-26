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
        BroadcastMessage("Round ended!");
        foreach (var player in Utilities.GetPlayers())
        {
            var dndPlayer = CommonMethods.RetrievePlayer(player);
            var dndClassEnum = (constants.DndClass) dndPlayer.DndClassId;
            var dndSpecieEnum = (constants.DndSpecie) dndPlayer.DndSpecieId;
            
            List<DndClassSpecieEventFeatureContainer> features = new();
            foreach(var classSpecieEventFeature in PostEventCallbacks)
            {
                var feature = (DndClassSpecieEventFeature<EventRoundEnd>) classSpecieEventFeature; 
                if(
                    (feature.DndClass == dndClassEnum
                     || feature.DndSpecie == dndSpecieEnum
                    )
                    && feature.HookMode == HookMode.Post
                )
                    features.Add(feature);            
            }
            
            features = features.OrderBy(feature =>((DndClassSpecieEventFeature<EventRoundEnd>) feature).Priority).ToList();
            bool overrideFlag = false;
            if(!features.Any())
                DoDefaultPostCallback(@event.Winner, player, dndPlayer);
            foreach(var f in features)
            {
                var feature = (DndClassSpecieEventFeature<EventRoundEnd>) f;
                HookResult result = feature.Callback(@event, info);
                if (feature.Priority == DndClassSpecieEventPriority.Interrupts)
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
        RoundStart roundStartEvent = (RoundStart) DndEvent<EventRoundStart>.RetrieveEvent<EventRoundStart>();
        var xpEvents = roundStartEvent.XpRoundTracker[dndPlayer.DndPlayerId];
        
        if(winner == (int) player.Team)
            GrantPlayerExperience(dndPlayer, Dndcs2.RoundWonXP.Value, Dndcs2.RoundWonXP.Description, GetType().Name);
        
        if (xpEvents.Any())
        {
            MessagePlayer(player, $"You earned {xpEvents.Select(e => e.ExperienceAmount).Sum()} XP for:");
            foreach (var xpEvent in xpEvents.GroupBy(e => e.Reason).ToList())
            {
                var @event = xpEvent.First();
                int counts = xpEvents.Count(e => e.Reason == @event.Reason);
                int totalEventXp = xpEvents.Where(e => e.Reason == @event.Reason).Select(e => e.ExperienceAmount).Sum();
                
                string xpMessage = $" {ChatColors.White}{counts}x {@event.Reason} ({ChatColors.Green}{totalEventXp})";                
                
                player.PrintToChat(xpMessage);
                foreach(var e in xpEvents.Where(e => e.Reason ==@event.Reason))
                    CommonMethods.GrantExperience(player, e);
            }
        }        
    }
}
