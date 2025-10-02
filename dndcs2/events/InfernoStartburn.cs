using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dndcs2.Sql;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.events;

public class InfernoStartburn : DndEvent<EventInfernoStartburn>
{

    public InfernoStartburn() : base()
    {
        
    }

    public override HookResult DefaultPostHookCallback(EventInfernoStartburn @event, GameEventInfo info)
    {
        var inferno = Utilities.GetEntityFromIndex<CInferno>(@event.Entityid);
        // if (inferno == null)
        // {
        //     BroadcastMessage("Couldn't find inferno");
        //     return HookResult.Continue;
        // }
        //
        // var stuff = new List<string>();
        // stuff.Add("FireCount=" +  inferno.FireCount.ToString());
        // stuff.Add("InfernoType=" +  inferno.InfernoType.ToString());
        // stuff.Add("FireEffectTickBegin=" +  inferno.FireEffectTickBegin.ToString());
        // stuff.Add("FireLifetime=" +  inferno.FireLifetime.ToString());
        // stuff.Add("InPostEffectTime=" +  inferno.InPostEffectTime.ToString());
        // stuff.Add("FiresExtinguishCount=" +  inferno.FiresExtinguishCount.ToString());
        // stuff.Add("WasCreatedInSmoke=" +  inferno.WasCreatedInSmoke.ToString());
        // stuff.Add("Extent=" +  inferno.Extent.ToString());
        // stuff.Add("DamageTimer=" +  inferno.DamageTimer.ToString());
        // stuff.Add("DamageRampTimer=" +  inferno.DamageRampTimer.ToString());
        // stuff.Add("SplashVelocity=" +  inferno.SplashVelocity.ToString());
        // stuff.Add("InitialSplashVelocity=" +  inferno.InitialSplashVelocity.ToString());
        // stuff.Add("StartPos=" +  inferno.StartPos.ToString());
        // stuff.Add("OriginalSpawnLocation=" +  inferno.OriginalSpawnLocation.ToString());
        // stuff.Add("ActiveTimer=" +  inferno.ActiveTimer.ToString());
        // stuff.Add("FireSpawnOffset=" +  inferno.FireSpawnOffset.ToString());
        // stuff.Add("MaxFlames=" +  inferno.MaxFlames.ToString());
        // stuff.Add("SpreadCount=" +  inferno.SpreadCount.ToString());
        // stuff.Add("BookkeepingTimer=" +  inferno.BookkeepingTimer.ToString());
        // stuff.Add("NextSpreadTimer=" +  inferno.NextSpreadTimer.ToString());
        // stuff.Add("SourceItemDefIndex=" + inferno.SourceItemDefIndex.ToString());
        // foreach (var output in stuff)
        // {
        //     try
        //     {
        //         BroadcastMessage(output);
        //     }
        //     catch (ArgumentException)
        //     {
        //         BroadcastMessage($"Couldn't print output for {output.Split('=')[0]}");
        //     }
        // }
        
        return HookResult.Continue;
    }
}
