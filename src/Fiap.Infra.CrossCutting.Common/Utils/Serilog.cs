namespace Fiap.Infra.Utils;

public class Serilog
{
    public string[] Using { get; set; } = Array.Empty<string>();
    public MinimumLevelSettings MinimumLevel { get; set; } = new();
    public WriteToSettings[] WriteTo { get; set; } = Array.Empty<WriteToSettings>();
    public string[] Enrich { get; set; } = Array.Empty<string>();
}

public class MinimumLevelSettings
{
    public string Default { get; set; } = string.Empty;
    public Dictionary<string, string> Override { get; set; } = new();
}

public class WriteToSettings
{
    public string Name { get; set; } = string.Empty;
    public ArgsSettings Args { get; set; } = new();
}

public class ArgsSettings
{
    public string? OutputTemplate { get; set; }
    public string? Uri { get; set; }
    public LabelSettings[]? Labels { get; set; }
}

public class LabelSettings
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
