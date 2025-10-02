using System.Collections.Immutable;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using dndcs2.constants;
using static Dndcs2.messages.DndMessages;
using Dndcs2.dtos;
using Dndcs2.events;
using Dndcs2.Sql;
using Dndcs2.stats;
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
    
    public static void DamageTarget(CCSPlayerController attacker, CCSPlayerController victim, int amount, bool separateDamage = true, DamageTypes_t damageType = DamageTypes_t.DMG_BULLET)
    {
        var size = Schema.GetClassSize("CTakeDamageInfo");
        var ptr = Marshal.AllocHGlobal(size);
    
        for (var i = 0; i < size; i++)
            Marshal.WriteByte(ptr, i, 0);
    
        var damageInfo = new CTakeDamageInfo(ptr);
        var attackerInfo = new CAttackerInfo(attacker);
    
        Marshal.StructureToPtr(attackerInfo, new IntPtr(ptr.ToInt64() + 0x80), false);
    
        Schema.SetSchemaValue(damageInfo.Handle, "CTakeDamageInfo", "m_hInflictor", attacker.Pawn.Raw);
        Schema.SetSchemaValue(damageInfo.Handle, "CTakeDamageInfo", "m_hAttacker", attacker.Pawn.Raw);
        Schema.SetSchemaValue(damageInfo.Handle, "CTakeDamageInfo", "m_bitsDamageType", damageType);
    
        damageInfo.Damage = amount;
    
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Invoke(victim.Pawn.Value, damageInfo);
        Marshal.FreeHGlobal(ptr);
    }
         
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
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate bool TraceShapeRayFilterDelegate(
        IntPtr GameTraceManager,
        Ray* trace,
        IntPtr vecStart,
        IntPtr vecEnd,
        CTraceFilter* traceFilter,
        CGameTrace* pGameTrace
    );

    public static Vector3 GetViewLocation(CCSPlayerController player, int distance = 8192, int cutShortDistance = 0)
    {
        var eyePosition = player.PlayerPawn.Value.AbsOrigin;
        Vector startOrigin = new Vector(eyePosition.X, eyePosition.Y, eyePosition.Z + player.PlayerPawn.Value.ViewOffset.Z);

        QAngle eyeAngles = player.PlayerPawn.Value.EyeAngles;
            
        Vector forward = new();
            
        NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, 0, 0);
        Vector endOrigin = new(startOrigin.X + forward.X * distance, startOrigin.Y + forward.Y * distance, startOrigin.Z + forward.Z * distance);
        
        unsafe
        {            
            CGameTrace* trace = stackalloc CGameTrace[1];
            var gameTraceManager = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("GameTraceManager"));
            int code = *(int*)(gameTraceManager + 3);            
            IntPtr gameTraceManagerAddress = gameTraceManager + code + 7;;

            IntPtr traceFunc = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("TraceFunc"));
            var traceShape = Marshal.GetDelegateForFunctionPointer<TraceShapeDelegate>(traceFunc);
            ulong mask = (ulong) (RaytraceMasks.Solid | RaytraceMasks.Window | RaytraceMasks.Debris | RaytraceMasks.Hitbox);
            ulong targetMask = (ulong) RaytraceMasks.Sky;
            
            var buyzoneHandles = Utilities.FindAllEntitiesByDesignerName<CBuyZone>("func_buyzone")
                .Select(e => (IntPtr) e.Handle).ToList();
            var bombsiteHandles = Utilities.FindAllEntitiesByDesignerName<CBombTarget>("func_bomb_target")
                .Select(e => (IntPtr) e.Handle).ToList();
            var filters = new List<IntPtr>()
            {
                player.PlayerPawn.Value.Handle
            };
            filters.AddRange(buyzoneHandles);
            filters.AddRange(bombsiteHandles);

            traceShape(*(IntPtr*)gameTraceManagerAddress, startOrigin.Handle, endOrigin.Handle, player.PlayerPawn.Value.Handle, ~0ul,
                    targetMask, trace);

            CGameTrace? possibleTraceResult = *trace;
            if (!possibleTraceResult.HasValue)
                return (Vector3) startOrigin;

            var traceResult = (CGameTrace)possibleTraceResult;

            CCSPlayerController? target = null;
            if ((CBaseEntity?)Activator.CreateInstance(typeof(CBaseEntity), traceResult.HitEntity) is
                { } entityInstance)
            {
                if (entityInstance == null)
                    return (Vector3) startOrigin;
                return new Vector3(
                    traceResult.EndPos.X + forward.X * -cutShortDistance, 
                    traceResult.EndPos.Y + forward.Y * -cutShortDistance, 
                    traceResult.EndPos.Z + forward.Z * -cutShortDistance
                );
                                        
            }
        }        
        
        return (Vector3) startOrigin;
    }

    public static CCSPlayerController? GetViewPlayer(CCSPlayerController player)
    {
        ulong mask = (ulong) (RaytraceMasks.Solid | RaytraceMasks.Player | RaytraceMasks.Npc | RaytraceMasks.Window | RaytraceMasks.Debris |
                              RaytraceMasks.Hitbox);
        ulong targetMask = (ulong) RaytraceMasks.Player;

        var buyzoneHandles = Utilities.FindAllEntitiesByDesignerName<CBuyZone>("func_buyzone")
            .Select(e => (IntPtr) e.Handle).ToList();
        var bombsiteHandles = Utilities.FindAllEntitiesByDesignerName<CBombTarget>("func_bomb_target")
            .Select(e => (IntPtr) e.Handle).ToList();
        var filters = new List<IntPtr>()
        {
            player.PlayerPawn.Value.Handle
        };
        filters.AddRange(buyzoneHandles);
        filters.AddRange(bombsiteHandles);        
        
        foreach(IntPtr filter in filters)
        {
            var target = Raytrace(player, mask, targetMask, filter);
            if (target != null)
                return target;
        }

        return null;
    }
    
    public static CCSPlayerController? Raytrace(CCSPlayerController player, ulong mask, ulong targetMask, IntPtr skip)
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
            startOrigin = new(startOrigin.X + forward.X * 40, startOrigin.Y + forward.Y * 40, startOrigin.Z + forward.Z * 40);
            Vector endOrigin = new(startOrigin.X + forward.X * 8192, startOrigin.Y + forward.Y * 8192, startOrigin.Z + forward.Z * 8192);
            
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

            CCSPlayerController? target = null;
            if ((CCSPlayerPawn?)Activator.CreateInstance(typeof(CCSPlayerPawn), traceResult.HitEntity) is
                { } entityInstance)
            {
                if(entityInstance.DesignerName.Equals("player"))
                    return entityInstance.OriginalController.Value;
            }                
            
            return null;                         
        }
    }
    
    public static bool IsPlayerStuck(CCSPlayerController player)
    {
        unsafe
        {
            var pawn = player.PlayerPawn.Value;
            Vector origin = pawn.AbsOrigin!;
            
            var pawnCollisionMins = pawn.Collision.Mins;
            var pawnCollisionMaxs = pawn.Collision.Maxs;
            var ray = new Ray(
                new Vector3(pawnCollisionMins.X, pawnCollisionMins.Y, pawnCollisionMins.Z),
                new Vector3(pawnCollisionMaxs.X, pawnCollisionMaxs.Y, pawnCollisionMaxs.Z)            
            );
            
            CTraceFilter filter = new CTraceFilter(pawn.Index, pawn.Index)
            {
                m_nObjectSetMask = 0xf,
                m_nCollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_PLAYER_MOVEMENT,
                m_nInteractsWith = pawn.Collision.CollisionAttribute.InteractsWith,
                m_nInteractsExclude = 0,
                m_nBits = 11,
                m_bIterateEntities = true,
                m_nInteractsAs = 0x40000
            };        
            filter.m_nHierarchyIds[0] = pawn.Collision.CollisionAttribute.HierarchyId;
            filter.m_nHierarchyIds[1] = 0;            
            
            IntPtr traceShape = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("TraceShape"));
            var traceShapeRayFilter = Marshal.GetDelegateForFunctionPointer<TraceShapeRayFilterDelegate>(traceShape);            
            
            CGameTrace* _trace = stackalloc CGameTrace[1];
            CTraceFilter* _filter = stackalloc CTraceFilter[1];
            
            IntPtr cTraceFilterVTable = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("CTraceFilterVtable"));
            if (cTraceFilterVTable == IntPtr.Zero)
                throw new Exception("Failed to find cTraceFilterVTable signature.");
            int filterCode = *(int*)(cTraceFilterVTable + 3);
            IntPtr _vtable = cTraceFilterVTable + filterCode + 7;
            
            var gameTraceManager = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("GameTraceManager"));
            if (gameTraceManager == IntPtr.Zero)
                throw new Exception("Failed to find gameTraceManager signature.");
            int code = *(int*)(gameTraceManager + 3);            
            IntPtr gameTraceManagerAddress = gameTraceManager + code + 7;;
            
            *_filter = filter;
            _filter->Vtable = (void*)_vtable;
            
            traceShapeRayFilter(
                *(nint*) gameTraceManagerAddress, 
                &ray, 
                origin.Handle,
                origin.Handle, 
                _filter,                 
                _trace
            );
            var gameTrace = (*_trace);
            var entityInstance = (CEntityInstance?)Activator.CreateInstance(typeof(CCSPlayerPawn), gameTrace.HitEntity);
            foreach (var entity in Utilities.GetAllEntities())
            {
                if(entity.Index == entityInstance.Index)
                    MessagePlayer(player, entity.DesignerName);
            }
            var result = (*_trace).Fraction;
            // MessagePlayer(player, $"{result}");
        }

        return false;
    }

    public static void SpawnSmokeGrenade(Vector position, QAngle angle, Vector velocity, CsTeam team)
    {
        var grenade = CSmokeGrenadeProjectile_CreateFunc.Invoke(
            position!.Handle,
            angle!.Handle,
            velocity!.Handle,
            velocity.Handle,
            IntPtr.Zero,
            45,
            (int)team);
    }
    
    public static CMolotovProjectile SpawnMolotovGrenade(Vector position, QAngle angle, Vector velocity, CsTeam team)
    {
        var grenade = CMolotovProjectile_CreateFunc.Invoke(
            position!.Handle,
            angle!.Handle,
            velocity!.Handle,
            velocity.Handle,
            IntPtr.Zero,
            46
        );
        
        grenade.Teleport(position, angle, velocity);
        grenade.InitialPosition.X = position.X;
        grenade.InitialPosition.Y = position.Y;
        grenade.InitialPosition.Z = position.Z;
        
        grenade.InitialVelocity.X = velocity.X;
        grenade.InitialVelocity.Y = velocity.Y;
        grenade.InitialVelocity.Z = velocity.Z;
        
        grenade.AngVelocity.X = velocity.X;
        grenade.AngVelocity.Y = velocity.Y;
        grenade.AngVelocity.Z = velocity.Z;
        grenade.TeamNum = (byte)team;
        return grenade;
    }
    
    public static void SpawnHeGrenade(Vector position, QAngle angle, Vector velocity, CsTeam team)
    {
        var grenade = CHEGrenadeProjectile_CreateFunc.Invoke(
            position!.Handle,
            angle!.Handle,
            velocity!.Handle,
            velocity.Handle,
            IntPtr.Zero,
            44
        );
        
        grenade.Teleport(position, angle, velocity);
        grenade.InitialPosition.X = position.X;
        grenade.InitialPosition.Y = position.Y;
        grenade.InitialPosition.Z = position.Z;
        
        grenade.InitialVelocity.X = velocity.X;
        grenade.InitialVelocity.Y = velocity.Y;
        grenade.InitialVelocity.Z = velocity.Z;
        
        grenade.AngVelocity.X = velocity.X;
        grenade.AngVelocity.Y = velocity.Y;
        grenade.AngVelocity.Z = velocity.Z;
        grenade.TeamNum = (byte)team;
    }
    
    public static void SpawnDecoyGrenade(Vector position, QAngle angle, Vector velocity, CsTeam team)
    {
        var grenade = CDecoyProjectile_CreateFunc.Invoke(
            position!.Handle,
            angle!.Handle,
            velocity!.Handle,
            velocity.Handle,
            IntPtr.Zero,
            47
        );
        
        grenade.Teleport(position, angle, velocity);
        grenade.InitialPosition.X = position.X;
        grenade.InitialPosition.Y = position.Y;
        grenade.InitialPosition.Z = position.Z;
        
        grenade.InitialVelocity.X = velocity.X;
        grenade.InitialVelocity.Y = velocity.Y;
        grenade.InitialVelocity.Z = velocity.Z;
        
        grenade.AngVelocity.X = velocity.X;
        grenade.AngVelocity.Y = velocity.Y;
        grenade.AngVelocity.Z = velocity.Z;
        grenade.TeamNum = (byte)team;
    }
    
    public static void SpawnFlashbang(Vector position, QAngle angle, Vector velocity, CsTeam team)
    {
        var newPosition = new Vector(position.X, position.Y, position.Z);
        var newAngle = new QAngle(angle.X, angle.Y, angle.Z);
        var newVelocity = new Vector(velocity.X, velocity.Y, velocity.Z);
        var newTeam = (int)team;
        Server.NextFrame(() =>
        {
            var grenade = Utilities.CreateEntityByName<CFlashbangProjectile>("flashbang_projectile");
            grenade.Teleport(position, angle, velocity);
            grenade.InitialPosition.X = newPosition.X;
            grenade.InitialPosition.Y = newPosition.Y;
            grenade.InitialPosition.Z = newPosition.Z;
        
            grenade.InitialVelocity.X = newVelocity.X;
            grenade.InitialVelocity.Y = newVelocity.Y;
            grenade.InitialVelocity.Z = newVelocity.Z;
        
            grenade.AngVelocity.X = newVelocity.X;
            grenade.AngVelocity.Y = newVelocity.Y;
            grenade.AngVelocity.Z = newVelocity.Z;
            
            grenade.TeamNum = (byte)newTeam; 
        });
    }
    
    public static MemoryFunctionWithReturn<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int, int, CSmokeGrenadeProjectile>
        CSmokeGrenadeProjectile_CreateFunc = new(
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? @"55 4C 89 C1 48 89 E5 41 57 49 89 FF 41 56 45 89 CE"
                : @"48 8B C4 48 89 58 ? 48 89 68 ? 48 89 70 ? 57 41 56 41 57 48 81 EC ? ? ? ? 48 8B B4 24 ? ? ? ? 4D 8B F8"
        );

    public static MemoryFunctionWithReturn<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int, CHEGrenadeProjectile>
        CHEGrenadeProjectile_CreateFunc = new(
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "55 4C 89 C1 48 89 E5 41 57 49 89 FF 41 56 49 89 D6"
                : "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 40 48 8B 6C 24 70"
        );
    
    public static MemoryFunctionWithReturn<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int, CMolotovProjectile>
        CMolotovProjectile_CreateFunc = new(
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "55 48 8D 05 ? ? ? ? 48 89 E5 41 57 41 56 41 55 41 54 49 89 FC 53 48 81 EC ? ? ? ? 4C 8D 35"
                : "48 8B C4 48 89 58 10 4C 89 40 18 48 89 48 08"
        );
    
    public static MemoryFunctionWithReturn<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int, CHEGrenadeProjectile>
        CDecoyProjectile_CreateFunc = new(
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "55 4C 89 C1 48 89 E5 41 57 45 89 CF 41 56 49 89 FE 41 55 49 89 D5 48 89 F2 48 89 FE 41 54 48 8D 3D 72 EA 84 FF"
                : "48 8B C4 55 56 48 81 EC 58 01 00 00"
        );
    
    public static Vector3 PlaceGrenade(Vector3 location)
    {
        var grenadeLocation = new Vector(location.X, location.Y, location.Z);
        QAngle eyeAngles = new QAngle(90, 0, 0);
            
        Vector forward = new();
        NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, 0, 0);
        Vector newLocation = new Vector(grenadeLocation.X + forward.X * 8192, grenadeLocation.Y + forward.Y * 8192, grenadeLocation.Z + forward.Z * 8192);
        
        unsafe
        {            
            CGameTrace* trace = stackalloc CGameTrace[1];
            var gameTraceManager = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("GameTraceManager"));
            int code = *(int*)(gameTraceManager + 3);            
            IntPtr gameTraceManagerAddress = gameTraceManager + code + 7;;

            IntPtr traceFunc = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("TraceFunc"));
            var traceShape = Marshal.GetDelegateForFunctionPointer<TraceShapeDelegate>(traceFunc);
            ulong mask = (ulong) (RaytraceMasks.Solid | RaytraceMasks.Window | RaytraceMasks.Debris | RaytraceMasks.Hitbox | RaytraceMasks.Player | RaytraceMasks.Npc);
            ulong targetMask = (ulong) RaytraceMasks.Sky;
            
            var buyzoneHandles = Utilities.FindAllEntitiesByDesignerName<CBuyZone>("func_buyzone")
                .Select(e => (IntPtr) e.Handle).ToList();
            var bombsiteHandles = Utilities.FindAllEntitiesByDesignerName<CBombTarget>("func_bomb_target")
                .Select(e => (IntPtr) e.Handle).ToList();
    
            traceShape(*(IntPtr*)gameTraceManagerAddress, grenadeLocation.Handle, newLocation.Handle, 0, ~0ul,
                    targetMask, trace);

            CGameTrace? possibleTraceResult = *trace;
            if (!possibleTraceResult.HasValue)
            {
                BroadcastMessage("No raytrace successful");
                return (Vector3)location;
            }

            var traceResult = (CGameTrace)possibleTraceResult;

            CCSPlayerController? target = null;
            if ((CBaseEntity?)Activator.CreateInstance(typeof(CBaseEntity), traceResult.HitEntity) is
                { } entityInstance)
            {
                BroadcastMessage(entityInstance.DesignerName);
                if (entityInstance == null)
                {
                    BroadcastMessage("No raytrace successful");
                    return (Vector3)location;
                }

                return new Vector3(
                    traceResult.EndPos.X, 
                    traceResult.EndPos.Y, 
                    traceResult.EndPos.Z + 150
                );
                                        
            }
        }        
        
        BroadcastMessage("No raytrace successful");
        return location;
    }

    public static void SpawnInfernoGraphic(CCSPlayerController attacker, Vector location)
    {
        var playerStats = PlayerStats.GetPlayerStats(attacker);
        location = new Vector(location.X, location.Y, location.Z);
        playerStats.InfernoLocation = location;
        if (location == null)
            return;

        var grenade = Dndcs2.SpawnMolotovGrenade(location, new QAngle(0, 0, 0), new Vector(0, 0, 0), attacker.Team);
        grenade.DetonateTime = 0;
        grenade.Thrower.Raw = attacker.PlayerPawn.Raw;

        Server.NextFrame(() =>
        {
            var infernoLocations = Utilities.GetPlayers()
                .Where(p => PlayerStats.GetPlayerStats(p).InfernoLocation != null)
                .Select(p => PlayerStats.GetPlayerStats(p).InfernoLocation).ToList();

            var infernos = Utilities.GetAllEntities()
                .Where(e => e.DesignerName == "inferno");
            if (!infernos.Any())
                throw new ArgumentException("Could not find the original casting location");

            foreach (var i in infernos)
            {
                var inferno = Utilities.GetEntityFromIndex<CInferno>((int)i.Index);
                if (inferno == null)                
                    continue;                

                infernoLocations.ForEach(l =>
                {
                    if (Vector3.Distance((Vector3)inferno.AbsOrigin, (Vector3)location) > 20)
                        return;                    
                    inferno.Remove();
                });
            }
        });
    }
}