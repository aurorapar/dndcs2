namespace dndcs2.constants;

public enum RaytraceMasks : ulong
{
    /// <summary>Empty</summary>
    Empty = 0,

    /// <summary>Solid</summary>
    Solid = 1ul << LayerIndex.Solid,
    /// <summary>Hitbox</summary>
    Hitbox = 1ul << LayerIndex.Hitbox,
    /// <summary>Trigger</summary>
    Trigger = 1ul << LayerIndex.Trigger,
    /// <summary>Sky</summary>
    Sky = 1ul << LayerIndex.Sky,

    /// <summary>PlayerClip</summary>
    PlayerClip = 1ul << StandardLayerIndex.PlayerClip,
    /// <summary>NpcClip</summary>
    NpcClip = 1ul << StandardLayerIndex.NpcClip,
    /// <summary>BlockLos</summary>
    BlockLos = 1ul << StandardLayerIndex.BlockLos,
    /// <summary>BlockLight</summary>
    BlockLight = 1ul << StandardLayerIndex.BlockLight,
    /// <summary>Ladder</summary>
    Ladder = 1ul << StandardLayerIndex.Ladder,
    /// <summary>Pickup</summary>
    Pickup = 1ul << StandardLayerIndex.Pickup,
    /// <summary>BlockSound</summary>
    BlockSound = 1ul << StandardLayerIndex.BlockSound,
    /// <summary>NoDraw</summary>
    NoDraw = 1ul << StandardLayerIndex.NoDraw,
    /// <summary>Window</summary>
    Window = 1ul << StandardLayerIndex.Window,
    /// <summary>PassBullets</summary>
    PassBullets = 1ul << StandardLayerIndex.PassBullets,
    /// <summary>WorldGeometry</summary>
    WorldGeometry = 1ul << StandardLayerIndex.WorldGeometry,
    /// <summary>Water</summary>
    Water = 1ul << StandardLayerIndex.Water,
    /// <summary>Slime</summary>
    Slime = 1ul << StandardLayerIndex.Slime,
    /// <summary>TouchAll</summary>
    TouchAll = 1ul << StandardLayerIndex.TouchAll,
    /// <summary>Player</summary>
    Player = 1ul << StandardLayerIndex.Player,
    /// <summary>Npc</summary>
    Npc = 1ul << StandardLayerIndex.Npc,
    /// <summary>Debris</summary>
    Debris = 1ul << StandardLayerIndex.Debris,
    /// <summary>PhysicsProp</summary>
    PhysicsProp = 1ul << StandardLayerIndex.PhysicsProp,
    /// <summary>NavIgnore</summary>
    NavIgnore = 1ul << StandardLayerIndex.NavIgnore,
    /// <summary>NavLocalIgnore</summary>
    NavLocalIgnore = 1ul << StandardLayerIndex.NavLocalIgnore,
    /// <summary>PostProcessingVolume</summary>
    PostProcessingVolume = 1ul << StandardLayerIndex.PostProcessingVolume,
    /// <summary>UnusedLayer3</summary>
    UnusedLayer3 = 1ul << StandardLayerIndex.UnusedLayer3,
    /// <summary>CarriedObject</summary>
    CarriedObject = 1ul << StandardLayerIndex.CarriedObject,
    /// <summary>Pushaway</summary>
    Pushaway = 1ul << StandardLayerIndex.Pushaway,
    /// <summary>ServerEntityOnClient</summary>
    ServerEntityOnClient = 1ul << StandardLayerIndex.ServerEntityOnClient,
    /// <summary>CarriedWeapon</summary>
    CarriedWeapon = 1ul << StandardLayerIndex.CarriedWeapon,
    /// <summary>StaticLevel</summary>
    StaticLevel = 1ul << StandardLayerIndex.StaticLevel,

    /// <summary>CsgoTeam1</summary>
    CsgoTeam1 = 1ul << CsgoLayerIndex.Team1,
    /// <summary>CsgoTeam2</summary>
    CsgoTeam2 = 1ul << CsgoLayerIndex.Team2,
    /// <summary>CsgoGrenadeClip</summary>
    CsgoGrenadeClip = 1ul << CsgoLayerIndex.GrenadeClip,
    /// <summary>CsgoDroneClip</summary>
    CsgoDroneClip = 1ul << CsgoLayerIndex.DroneClip,
    /// <summary>CsgoMoveable</summary>
    CsgoMoveable = 1ul << CsgoLayerIndex.Moveable,
    /// <summary>CsgoOpaque</summary>
    CsgoOpaque = 1ul << CsgoLayerIndex.Opaque,
    /// <summary>CsgoMonster</summary>
    CsgoMonster = 1ul << CsgoLayerIndex.Monster,
    /// <summary>CsgoUnusedLayer</summary>
    CsgoUnusedLayer = 1ul << CsgoLayerIndex.UnusedLayer,
    /// <summary>CsgoThrownGrenade</summary>
    CsgoThrownGrenade = 1ul << CsgoLayerIndex.ThrownGrenade,
}

public enum LayerIndex
{
    /// <summary>Solid objects layer</summary>
    Solid = 0,

    /// <summary>Hitbox collision layer</summary>
    Hitbox,

    /// <summary>Trigger volume layer</summary>
    Trigger,

    /// <summary>Skybox layer</summary>
    Sky,

    /// <summary>First available layer for user-defined content</summary>
    FirstUser,

    /// <summary>Special value indicating layer not found</summary>
    NotFound = -1,

    /// <summary>Maximum allowed layer index</summary>
    MaxAllowed = 64
}

public enum StandardLayerIndex
{
    /// <summary>PlayerClip</summary>
    PlayerClip = LayerIndex.FirstUser,
    /// <summary>NpcClip</summary>
    NpcClip,
    /// <summary>BlockLos</summary>
    BlockLos,
    /// <summary>BlockLight</summary>
    BlockLight,
    /// <summary>Ladder</summary>
    Ladder,
    /// <summary>Pickup</summary>
    Pickup,
    /// <summary>BlockSound</summary>
    BlockSound,
    /// <summary>NoDraw</summary>
    NoDraw,
    /// <summary>Window</summary>
    Window,
    /// <summary>PassBullets</summary>
    PassBullets,
    /// <summary>WorldGeometry</summary>
    WorldGeometry,
    /// <summary>Water</summary>
    Water,
    /// <summary>Slime</summary>
    Slime,
    /// <summary>TouchAll</summary>
    TouchAll,
    /// <summary>Player</summary>
    Player,
    /// <summary>Npc</summary>
    Npc,
    /// <summary>Debris</summary>
    Debris,
    /// <summary>PhysicsProp</summary>
    PhysicsProp,
    /// <summary>NavIgnore</summary>
    NavIgnore,
    /// <summary>NavLocalIgnore</summary>
    NavLocalIgnore,
    /// <summary>PostProcessingVolume</summary>
    PostProcessingVolume,
    /// <summary>UnusedLayer3</summary>
    UnusedLayer3,
    /// <summary>CarriedObject</summary>
    CarriedObject,
    /// <summary>Pushaway</summary>
    Pushaway,
    /// <summary>ServerEntityOnClient</summary>
    ServerEntityOnClient,
    /// <summary>CarriedWeapon</summary>
    CarriedWeapon,
    /// <summary>StaticLevel</summary>
    StaticLevel,
    /// <summary>FirstModSpecific</summary>
    FirstModSpecific
}

public enum CsgoLayerIndex
{
    /// <summary>Team 1 layer</summary>
    Team1 = StandardLayerIndex.FirstModSpecific,

    /// <summary>Team 2 layer</summary>
    Team2,

    /// <summary>Grenade collision layer</summary>
    GrenadeClip,

    /// <summary>Drone collision layer</summary>
    DroneClip,

    /// <summary>Movable physics objects layer</summary>
    Moveable,

    /// <summary>Opaque surfaces layer</summary>
    Opaque,

    /// <summary>Monster/NPC layer</summary>
    Monster,

    /// <summary>Unused/reserved layer</summary>
    UnusedLayer,

    /// <summary>Thrown grenade entities layer</summary>
    ThrownGrenade
}