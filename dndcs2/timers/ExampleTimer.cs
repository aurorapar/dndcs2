using CounterStrikeSharp.API.Modules.Cvars;
using static Dndcs2.messages.DndMessages;

namespace Dndcs2.timers;

public class ExampleTimer : DndTimer
{
    public ExampleTimer() : base(
        (int) float.Parse(ConVar.Find("mp_roundtime").StringValue) * 60 + 
                                      (float.Parse(ConVar.Find("mp_roundtime").StringValue) - (int) (float.Parse(ConVar.Find("mp_roundtime").StringValue))) * 60, 
        5.0f, null)
    {
        
    }

    public override void Fire()
    {
        BroadcastMessage("This is an announcement!");
    }
}