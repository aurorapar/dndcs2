using System.Collections.Immutable;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using dndcs2.constants;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.@struct;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Dndcs2;

public partial class Dndcs2
{
    public static ImmutableList<string> Pistols = ImmutableList.Create(
        "glock","hkp2000","usp_silencer","elite","p250","tec9","fiveseven","cz75a","deagle","revolver"
    );

    public static ImmutableList<string> Shotguns = ImmutableList.Create(
        "nova", "mag7", "sawedoff", "xm1014"
    );
    public static ImmutableList<string> MGs = ImmutableList.Create(
        "m249", "negev"
    );
    public static ImmutableList<string> SMGs = ImmutableList.Create(
        "mp5sd", "p90", "mp7", "mac10", "mp9", "bizon", "ump45"
    );
    public static ImmutableList<string> Rifles = ImmutableList.Create(
        "galilar", "famas", "ak47", "m4a1", "m4a1_silencer", "ssg08", "aug", "sg556"
    );
    public static ImmutableList<string> Snipers = ImmutableList.Create(
        "awp", "g3sg1", "scar20"
    );
    public static ImmutableList<string> Grenades = ImmutableList.Create(
        "flashbang", "smokegrenade", "hegrenade", "molotov", "incgrenade", "decoy"
    );
    public static ImmutableList<string> Weapons = ImmutableList
        .Create("taser")
        .AddRange(Pistols)
        .AddRange(Shotguns)
        .AddRange(MGs)
        .AddRange(SMGs)
        .AddRange(Rifles)
        .AddRange(Snipers)
        .AddRange(Grenades)
    ;
         
    public static void ShowDndXp(CCSPlayerController player, CCSPlayerController target)
    {
        DndPlayer dndPlayer = CommonMethods.RetrievePlayer(target);
        MenuManager.CloseActiveMenu(player);
        Server.NextFrame(() =>
        {
            player.PrintToCenterHtml(GetPlayerStats(target, dndPlayer).Replace("\n", "<br>")); 
        });
    }
    
    public static void ShowDndInfo(CCSPlayerController player)
    {
        string source = "https://dndcs2.spawningpool.net/info.html";
        HttpClient client = new HttpClient();
        string page = client.GetStringAsync(source).Result;
        
        MenuManager.CloseActiveMenu(player);
        Server.NextFrame(() =>
        {
            player.PrintToCenterHtml(page, 10); 
        });        
    }

    public static void ProcessPlayerXp(CCSPlayerController player)
    {
        var dndPlayer = CommonMethods.RetrievePlayer(player);
        RoundStart roundStartEvent = (RoundStart) DndEvent<EventRoundStart>.RetrieveEvent<EventRoundStart>();
        if (!roundStartEvent.XpRoundTracker.ContainsKey(dndPlayer.DndPlayerId))
            roundStartEvent.XpRoundTracker[dndPlayer.DndPlayerId] = new();
        
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

    public static string? GetPlayerWeapon(CCSPlayerController player)
    {
        if (player.PlayerPawn.Value?.WeaponServices is null)
            return null;
        if (player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value is null)
            return null;
        return player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value?.DesignerName.Replace("weapon_", "");
    }

    public static void UpdatePrehookDamage(EventPlayerHurt @event, int newDamageAmount)
    {
        if (@event.Health > @event.DmgHealth)
        {
            @event.Health -= newDamageAmount - @event.DmgHealth;
            @event.Userid.PlayerPawn.Value.Health = @event.Health;
        }
    }

    public static void InitializeTrackers(CCSPlayerController player, DndPlayer dndPlayer)
    {
        RoundStart roundStartEvent = (RoundStart) DndEvent<EventRoundStart>.RetrieveEvent<EventRoundStart>();
        if(!roundStartEvent.XpRoundTracker.ContainsKey(dndPlayer.DndPlayerId))
            roundStartEvent.XpRoundTracker[dndPlayer.DndPlayerId] = new List<DndExperienceLog>();

        PlayerDeath playerDeathEvent = (PlayerDeath) DndEvent<EventPlayerDeath>.RetrieveEvent<EventPlayerDeath>();
        if (!playerDeathEvent.KillStreakTracker.ContainsKey(player))
            playerDeathEvent.KillStreakTracker[player] = 0;
    }

    public static string GetCurrentRoundTime()
    {        
        var elapsedTime = (DateTime.Now - Dndcs2.RoundTime);
        var roundTime = float.Parse(ConVar.Find("mp_roundtime").StringValue);
        elapsedTime -= TimeSpan.FromMinutes((int)roundTime);
        elapsedTime -= TimeSpan.FromSeconds((roundTime - (int)roundTime) * 60);        
        string message = $"{elapsedTime.Minutes}m {elapsedTime.Seconds}s";
        BroadcastMessage(message);
        return message;
    }
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate bool TraceShapeDelegate(
        IntPtr GameTraceManager,
        IntPtr vecStart,
        IntPtr vecEnd,
        IntPtr skip,
        ulong mask,
        ulong content,
        CGameTrace* pGameTrace
    );

    public static void RaytracePlayer(CCSPlayerController player)
    {
        ulong mask = (ulong) (RaytraceMasks.Solid | RaytraceMasks.Player | RaytraceMasks.Npc | RaytraceMasks.Window | RaytraceMasks.Debris |
                              RaytraceMasks.Hitbox);
        ulong targetMask = (ulong) RaytraceMasks.Player;
        var target = Raytrace<CCSPlayerPawn, CCSPlayerController>(player, mask, targetMask);
        if(target == null)
            return;
        MessagePlayer(player, $"You would hit {target.PlayerName}");
    }
    
    public static TU? Raytrace<T, TU>(CCSPlayerController player, ulong mask, ulong targetMask)
    where TU : CBaseEntity
    {
        unsafe
        {
            if (player == null) 
                return null;
            
            var eyePosition = player.PlayerPawn.Value.AbsOrigin;
            Vector startOrigin = new Vector(eyePosition.X, eyePosition.Y, eyePosition.Z + player.PlayerPawn.Value.ViewOffset.Z);

            QAngle eyeAngles = player.PlayerPawn.Value.EyeAngles;
            
            Vector forward = new();
            
            NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, 0, 0);
            Vector endOrigin = new(startOrigin.X + forward.X * 8192, startOrigin.Y + forward.Y * 8192, startOrigin.Z + forward.Z * 8192);

            

            IntPtr skip = player.PlayerPawn.Value.Handle;
            
            CGameTrace* trace = stackalloc CGameTrace[1];
            var gameTraceManager = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("GameTraceManager"));
            int code = *(int*)(gameTraceManager + 3);            
            IntPtr gameTraceManagerAddress = gameTraceManager + code + 7;;

            IntPtr traceFunc = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("TraceFunc"));
            var traceShape = Marshal.GetDelegateForFunctionPointer<TraceShapeDelegate>(traceFunc);
            traceShape(*(IntPtr*)gameTraceManagerAddress, startOrigin.Handle, endOrigin.Handle, skip, mask,targetMask, trace);
            
            CGameTrace? possibleTraceResult = *trace;
            if (!possibleTraceResult.HasValue)
                return null;

            var traceResult = (CGameTrace) possibleTraceResult;

            TU? target = null;
            if ((CCSPlayerPawn?)Activator.CreateInstance(typeof(T), traceResult.HitEntity) is
                { } entityInstance)
            {
                var test = ((CCSPlayerPawn)entityInstance).OriginalController.Value;
                if (test is TU)
                {
                    return (TU)Convert.ChangeType(test, typeof(TU));
                }                
            }            
        }

        return null;
    }

}