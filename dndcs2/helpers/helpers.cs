using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;

namespace Dndcs2;

public partial class Dndcs2
{
    public static void ShowDndXp(CCSPlayerController player, CCSPlayerController target)
    {
        DndPlayer dndPlayer = CommonMethods.RetrievePlayer(target);
        player.PrintToCenterHtml(GetPlayerStats(target, dndPlayer).Replace("\n", "<br>"));
    }
    
    public static void ShowDndInfo(CCSPlayerController player)
    {
        string source = "https://dndcs2.spawningpool.net/info.html";
        HttpClient client = new HttpClient();
        string page = client.GetStringAsync(source).Result;
        player.PrintToCenterHtml(page, 10);
    }

    public static void ProcessPlayerXp(CCSPlayerController player)
    {
        var dndPlayer = CommonMethods.RetrievePlayer(player);
        RoundStart roundStartEvent = (RoundStart) DndEvent<EventRoundStart>.RetrieveEvent<EventRoundStart>();
        var xpEvents = roundStartEvent.XpRoundTracker[dndPlayer.DndPlayerId];
        
        if (xpEvents.Any())
        {
            MessagePlayer(player, $"You earned {xpEvents.Select(e => e.ExperienceAmount).Sum()} XP for:");
            foreach (var xpEvent in xpEvents.GroupBy(e => e.Reason).ToList())
            {
                var @event = xpEvent.First();
                int counts = xpEvents.Count(e => e.Reason == @event.Reason);
                int totalEventXp = xpEvents.Where(e => e.Reason == @event.Reason).Select(e => e.ExperienceAmount).Sum();
                
                string xpMessage = $" {ChatColors.White}{counts}x {@event.Reason} ({ChatColors.Green}{totalEventXp}{ChatColors.White})";                
                
                player.PrintToChat(xpMessage);
                foreach(var e in xpEvents.Where(e => e.Reason ==@event.Reason))
                    CommonMethods.GrantExperience(player, e);
            }
        }

        roundStartEvent.XpRoundTracker[dndPlayer.DndPlayerId] = new();
    }
}