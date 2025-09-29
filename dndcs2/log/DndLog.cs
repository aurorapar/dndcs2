using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dndcs2.log;

public class DndLog
{
    private readonly ILogger<DndLog> _logger;

    public DndLog(ILogger<DndLog> logger)
    {
        _logger = logger;
    }

    public void LogError(string logMessage)
    {
        _logger.LogError(logMessage);
    }
    
    public void LogInformation(string logMessage)
    {
        _logger.LogInformation(logMessage);
    }
    
    public void Debug(string logMessage)
    {
        _logger.LogDebug(logMessage);
    }
}

public class TestPluginServiceCollection : IPluginServiceCollection<Dndcs2>
{
    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<DndLog>();
    }
}