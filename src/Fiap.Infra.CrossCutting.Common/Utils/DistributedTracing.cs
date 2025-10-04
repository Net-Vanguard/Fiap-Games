namespace Fiap.Infra.Utils;

public class DistributedTracing
{
    public JaegerSettings Jaeger { get; set; } = new();
}

public class JaegerSettings
{
    public string ServiceName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
}