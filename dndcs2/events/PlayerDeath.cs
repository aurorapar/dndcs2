using CounterStrikeSharp.API.Core;
using static Dndcs2.messages.DndMessages;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.Sql;

namespace Dndcs2.events;

public class PlayerDeath : DndEvent<EventPlayerDeath>
{
    public Dictionary<CCSPlayerController, int> KillStreakTracker = new();

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

        KillStreakTracker[victim] = 0;
        if (!KillStreakTracker.ContainsKey(attacker))
            KillStreakTracker[attacker] = 0;
        if (attacker.Team != victim.Team)
        {
            KillStreakTracker[attacker] += 1;
            var victimLevel = CommonMethods.RetrievePlayerClassProgress(victim).DndLevelAmount;
            var attackerLevel = CommonMethods.RetrievePlayerClassProgress(attacker).DndLevelAmount;
            int amount = Dndcs2.KillXP.Value;
            string reason = Dndcs2.KillXP.Description;
            if (victimLevel != attackerLevel)
            {
                amount = 2 * (victimLevel - attackerLevel) + amount;
                if (attackerLevel > victimLevel)
                    reason += Dndcs2.KillModiferLowXP.Description;
                else 
                    reason += Dndcs2.KillModifierHighXP.Description;
            }
            
            GrantPlayerExperience(dndPlayerAttacker, amount, reason, GetType().Name);
            if(@event.Headshot)
                GrantPlayerExperience(dndPlayerAttacker, Dndcs2.HeadShotXP.Value, Dndcs2.HeadShotXP.Description, GetType().Name);
            if(@event.Weapon.ToLower().Contains("knife"))
                GrantPlayerExperience(dndPlayerAttacker, Dndcs2.KnifeXP.Value, Dndcs2.KnifeXP.Description, GetType().Name);
            else if(@event.Weapon.ToLower().Contains("grenade"))
                GrantPlayerExperience(dndPlayerAttacker, Dndcs2.GrenadeXP.Value, Dndcs2.GrenadeXP.Description, GetType().Name);
        }
        else
        {
            // Doing this immediately so that people get the *right* idea 
            MessagePlayer(attacker, string.Format("You lost {0} experience for {1}", Dndcs2.TeamKillXP.Value, Dndcs2.TeamKillXP.Description));
            var xpLogItem = new DndExperienceLog(GetType().Name, DateTime.UtcNow, GetType().Name, DateTime.UtcNow, 
                true, dndPlayerAttacker.DndPlayerId, Dndcs2.TeamKillXP.Value, Dndcs2.TeamKillXP.Description);
            CommonMethods.GrantExperience(attacker, xpLogItem);
        }
        
        return HookResult.Continue;
    }
}
