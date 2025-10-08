using CounterStrikeSharp.API.Core;
using Dndcs2.dtos;
using Dndcs2.Sql;

namespace Dndcs2;

public partial class Dndcs2
{
    public static string GetTotalPlayTime(DndPlayer dndPlayer)
    {
        double totalPlayTime = dndPlayer.PlayTimeHours + (DateTime.UtcNow - dndPlayer.LastConnected).TotalHours;
        return $"Played: {(int) totalPlayTime} hours {(int) ((totalPlayTime - (int) totalPlayTime) * 60)} minutes";
    }

    public static string GetPlayerStats(CCSPlayerController player, DndPlayer dndPlayer)
    {
        DndClassProgress classProgress = CommonMethods.RetrievePlayerClassProgress(player);
        DndSpecieProgress specieProgress = CommonMethods.RetrievePlayerSpecieProgress(player);

        string classMessage =
            $"Level {classProgress.DndLevelAmount} {(constants.DndClass)dndPlayer.DndClassId} ({classProgress.DndExperienceAmount}/{classProgress.DndLevelAmount * 1000})";
        string specieMessage =
            $"Level {specieProgress.DndLevelAmount} {(constants.DndSpecie)dndPlayer.DndSpecieId} ({specieProgress.DndExperienceAmount}/{specieProgress.DndLevelAmount * 1000})";
        string subclassMessage = "";
        if (dndPlayer.DndSubClassId != null)
        {
            DndSubClassProgress subClassProgress = CommonMethods.RetrievePlayerSubClassProgress(player);
            subclassMessage =
                $"\nLevel {subClassProgress.DndLevelAmount} {Dndcs2.FormatConstantNameForDisplay(((constants.DndSubClass) dndPlayer.DndSubClassId).ToString())} ({subClassProgress.DndExperienceAmount}/{subClassProgress.DndLevelAmount * 1000})";
        }

        return $"<font size=\"24\">D&D Stats for {player.PlayerName}\n{GetTotalPlayTime(dndPlayer)}\n{classMessage}\n{specieMessage}{subclassMessage}</font>";
    }
}