using Dalamud.Configuration;
using ECommons.DalamudServices;
using Newtonsoft.Json;

namespace Auctioneer;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool RunAsPostTask { get; set; } = false;

    public static Configuration Load()
    {
        var path = Svc.PluginInterface.ConfigFile.FullName;
        if (!File.Exists(path))
            return new Configuration();

        var json = File.ReadAllText(path);
        Configuration? config = JsonConvert.DeserializeObject<Configuration>(json);
        return config ?? new Configuration();
    }

    public void Save()
    {
        var path = Svc.PluginInterface.ConfigFile.FullName;
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(path, json);
    }
}