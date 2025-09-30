using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using Dndcs2.dice;
using static Dndcs2.messages.DndMessages;
namespace Dndcs2.events;

public static class OnTakeDamageHook 
{
    public static HookResult OnTakeDamage(DynamicHook hook)
    {
        return HookResult.Continue;
    }
}
