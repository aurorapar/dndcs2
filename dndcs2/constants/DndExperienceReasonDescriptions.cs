using Dndcs2.patterns;
using Microsoft.Extensions.Logging;

namespace Dndcs2.constants;

public class DndExperienceReasonDescriptions : Singleton
{
    public static Dictionary<DndExperienceReason, string> ReasonDescriptions = 
        new Dictionary<DndExperienceReason, string>();

    private DndExperienceReasonDescriptions()
    {
        ReasonDescriptions[DndExperienceReason.Kill] = "Scoring a kill";
        ReasonDescriptions[DndExperienceReason.Assist] = "Assisting with a kill";
        ReasonDescriptions[DndExperienceReason.TeamKill] = "Killing a teammate";
        
        foreach(var reason in Enum.GetValues(typeof(DndExperienceReason)).Cast<DndExperienceReason>())
        {
            if(!ReasonDescriptions.ContainsKey(reason))
                Dndcs2.DndLogger.LogError("Experience Reas");
        }
    }    
}

