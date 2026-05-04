namespace Latidos.Services;

/// <summary>
/// Configuration para elegir entre Mock y Real Services
/// </summary>
public interface IServiceConfiguration
{
    bool UseMockServices { get; }
}

public class ServiceConfiguration : IServiceConfiguration
{
    public bool UseMockServices { get; set; } = true; // Por defecto Mock para desarrollo

    public ServiceConfiguration(bool useMock = true)
    {
        UseMockServices = useMock;
    }
}
