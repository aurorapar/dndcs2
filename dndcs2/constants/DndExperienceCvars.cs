using CounterStrikeSharp.API.Modules.Cvars;

namespace Dndcs2;

public partial class Dndcs2
{
    public static FakeConVar<int> KillXP = new ("DndKillXP", "Scoring a kill!", 50);
    public static FakeConVar<int> KillModifierHighXP = new ("DndKillModifierHighXP", "(they were higher level!)", 50);
    public static FakeConVar<int> KillModiferLowXP = new ("DndKillModifierLowXP", " (they were lower level)", 50);
    public static FakeConVar<int> HeadShotXP = new ("DndHeadShotXP", "Getting a Headshot!", 35);
    public static FakeConVar<int> KnifeXP = new ("DndKnifeXP", "Getting a Knife kill!", 75);
    public static FakeConVar<int> GrenadeXP = new ("DndGrenadeXP", "Getting a Grenade kill!", 50);
    public static FakeConVar<int> KillingSpreeXP = new ("DndKillingSpreeXP", "Being on a killing spree!", 20);
    public static FakeConVar<int> BountyXP = new ("DnDBountyXP", "Collecting a bounty on {0}!", 40);
    public static FakeConVar<int> AssistXP = new ("DndAssistXP", "Assisting {0} with a kill!", 20);
    public static FakeConVar<int> TeamKillXP = new ("DndTeamKillXP", "Killing a teammate!", 200);
    public static FakeConVar<int> BombPlantedXP = new ("DndBombPlantedXP", "Planting the bomb!", 100);
    public static FakeConVar<int> BombExplodedXP = new ("DndBombExplodedXP", "Your bomb plant exploding!", 50);
    public static FakeConVar<int> BombDefusedXP = new ("Dnd.BombDefusedXP", "Defusing the bomb!", 100);
    public static FakeConVar<int> BonbDefusedWithEnemiesXP = new ("DndBombDefusedWithEnemiesXP", "Defusing the bomb while enemies are alive!", 150);
    public static FakeConVar<int> HostageRescuedXP = new ("DndHostageRescued", "Rescuing a hostage!", 50);
    public static FakeConVar<int> HostagedSecuredXP = new ("DndHostageSecured", "Preventing a hostage from being rescued!", 50);
    public static FakeConVar<int> HostageKilledXP = new ("DndHostageKilled", "Killing a hostage!", 35);
    public static FakeConVar<int> RoundWonXP = new ("DndRoundWonXP", "Winning the round!", 35);
    public static FakeConVar<int> BotKilledXP = new ("DndBotKilledXP", "Killing a bot.", 25);
}