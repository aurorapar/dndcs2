using CounterStrikeSharp.API.Core;

namespace Dndcs2.events.eventstack;

public static class EventFactory
{    
    public static void RegisterEventCallbacks()
    {
        var playerSpawnCallbacks = new PlayerSpawn<EventPlayerSpawn, EventPlayerSpawn, EventPlayerSpawn>();
        var playerConnectFullCallbacks = new PlayerConnectFull<EventPlayerConnectFull, EventPlayerConnectFull, EventPlayerConnectFull>();
        var playerDisconnectCallbacks = new PlayerDisconnect<EventPlayerDisconnect, EventPlayerDisconnect, EventPlayerDisconnect>();
    }
}