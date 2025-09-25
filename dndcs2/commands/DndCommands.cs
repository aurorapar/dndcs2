using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using static Dndcs2.messages.DndMessages;
using Dndcs2.constants;
using Dndcs2.dtos;
using Dndcs2.Sql;

namespace Dndcs2;

public partial class Dndcs2
{
    [ConsoleCommand("dndxp", "Shows player their XP")]
    public void DndXp(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) 
            return;        

        DndPlayer dndPlayer = CommonMethods.RetrievePlayer(player);
        DndClassProgress classProgress = CommonMethods.RetrievePlayerClassProgress(player);
        DndSpecieProgress specieProgress = CommonMethods.RetrievePlayerSpecieProgress(player);
        string message =
            $"Level {classProgress.DndLevelAmount} {(constants.DndClass)dndPlayer.DndClassId} ({classProgress.DndExperienceAmount}/{classProgress.DndLevelAmount * 1000})";
        MessagePlayer(player, message);
        message =
            $"Level {specieProgress.DndLevelAmount} {(constants.DndSpecie)dndPlayer.DndSpecieId} ({specieProgress.DndExperienceAmount}/{specieProgress.DndLevelAmount * 1000})";
        MessagePlayer(player, message);
    }
}