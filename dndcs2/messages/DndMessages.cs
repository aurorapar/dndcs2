using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Dndcs2.messages;

public static class DndMessages
{
    public static void MessagePlayer(CCSPlayerController player, string message)
    {
        player.PrintToChat($" {ChatColors.Green}[D&D]{ChatColors.Default} {message}");
    }

    public static void BroadcastMessage(string message)
    {
        Server.PrintToChatAll($" {ChatColors.Green}[D&D]{ChatColors.Default} {message}");
    }

    public static void PrintMessageToConsole(string message)
    {
        System.Console.WriteLine(new string('=', 50));
        System.Console.WriteLine($"[D&D] {message}");
        System.Console.WriteLine(new string('=', 50));
    }
}