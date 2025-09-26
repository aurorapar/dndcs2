using CounterStrikeSharp.API.Core;
using Dndcs2.dtos;
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
}