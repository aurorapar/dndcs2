using CounterStrikeSharp.API;
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

        List<EventCallbackFeatureContainer> features = new();
        foreach(var classSpecieEventFeature in PostEventCallbacks)
        {
            var feature = (EventCallbackFeature<EventPlayerDeath>) classSpecieEventFeature; 
            if(
                (feature.DndClass == victimClassEnum
                || feature.DndSpecie == victimSpecieEnum
                || (feature.DndClass == attackerClassEnum || feature.DndSpecie == attackerSpecieEnum) && ! attacker.ControllingBot)
                && feature.HookMode == HookMode.Post
            )
                features.Add(feature);
            
        }

        features = features.OrderBy(feature =>((EventCallbackFeature<EventPlayerDeath>) feature).CallbackFeaturePriority).ToList();
        bool overrideFlag = false;
        foreach(var f in features)
        {
            var feature = (EventCallbackFeature<EventPlayerDeath>) f;
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
        
        if (!KillStreakTracker.ContainsKey(attacker))
            KillStreakTracker[attacker] = 0;
        if (attacker.Team != victim.Team)
        {
            if (victim.IsBot)
            {
                GrantPlayerExperience(dndPlayerAttacker, Dndcs2.BotKilledXP.Value, Dndcs2.BotKilledXP.Description, GetType().Name);
                return HookResult.Continue;
            }

            if (!attacker.ControllingBot)
            {
                KillStreakTracker[attacker] += 1;
                var victimLevel = CommonMethods.RetrievePlayerClassLevel(victim);
                var attackerLevel = CommonMethods.RetrievePlayerClassLevel(attacker);
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
                if (@event.Headshot)
                    GrantPlayerExperience(dndPlayerAttacker, Dndcs2.HeadShotXP.Value, Dndcs2.HeadShotXP.Description,
                        GetType().Name);
                if (@event.Weapon.ToLower().Contains("knife"))
                    GrantPlayerExperience(dndPlayerAttacker, Dndcs2.KnifeXP.Value, Dndcs2.KnifeXP.Description,
                        GetType().Name);
                else if (@event.Weapon.ToLower().Contains("grenade"))
                    GrantPlayerExperience(dndPlayerAttacker, Dndcs2.GrenadeXP.Value, Dndcs2.GrenadeXP.Description,
                        GetType().Name);

                if (KillStreakTracker[victim] >= 3)
                {
                    GrantPlayerExperience(dndPlayerAttacker, Dndcs2.BountyXP.Value,
                        string.Format(Dndcs2.BountyXP.Description, victim.PlayerName),
                        GetType().Name);
                    BroadcastMessage(
                        $"A bounty was collected on {victim.PlayerName} for {Dndcs2.BountyXP.Value * KillStreakTracker[victim]}XP!");
                }

                if (KillStreakTracker[attacker] >= 3)
                {
                    GrantPlayerExperience(dndPlayerAttacker, Dndcs2.KillingSpreeXP.Value * KillStreakTracker[attacker],
                        Dndcs2.KillingSpreeXP.Description,
                        GetType().Name);
                    if (KillStreakTracker[attacker] == 3)
                        BroadcastMessage(
                            $"{attacker.PlayerName} is on a killing spree! Collect their bounty for {Dndcs2.BountyXP.Value * KillStreakTracker[attacker]}XP!");
                    else
                        BroadcastMessage(
                            $"The bounty on {attacker.PlayerName} is increasing! They're worth {Dndcs2.BountyXP.Value * KillStreakTracker[attacker]}XP!");
                }

                if (@event.Assister != null)
                {
                    var dndAssister = CommonMethods.RetrievePlayer(@event.Assister);
                    GrantPlayerExperience(dndAssister, Dndcs2.AssistXP.Value, String.Format(Dndcs2.AssistXP.Description,
                        attacker.PlayerName), GetType().Name);
                }
            }
        }
        else
        {
            if (Utilities.GetPlayers().Contains(attacker) && victim != attacker) // disconnecting counts as a suicide
            {
                Dndcs2.Instance.Log.LogInformation($"{attacker.PlayerName} team killed {victim.PlayerName}");
                MessagePlayer(attacker,
                    string.Format("You lost {0} experience for {1}", Dndcs2.TeamKillXP.Value,
                        Dndcs2.TeamKillXP.Description));
                var xpLogItem = new DndExperienceLog(GetType().Name, DateTime.UtcNow, GetType().Name, DateTime.UtcNow,
                    true, dndPlayerAttacker.DndPlayerId, Dndcs2.TeamKillXP.Value, Dndcs2.TeamKillXP.Description);
                CommonMethods.GrantExperience(attacker, xpLogItem);
            }
        }
        
        Dndcs2.ShowDndXp(victim, attacker);
        
        KillStreakTracker[victim] = 0;
        
        return HookResult.Continue;
    }
}
