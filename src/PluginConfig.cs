using BepInEx.Configuration;
using Newtonsoft.Json;
using System;

namespace QualityCompany;

[Serializable]
internal class PluginConfig
{
    public string SellIgnoreList { get; set; }

    [JsonIgnore]
    public bool ShowDebugLogs { get; set; }

    public PluginConfig()
    {
    }

    public void Bind(ConfigFile configFile)
    {
        SellIgnoreList = configFile.Bind(
            "Selling",
            "SellIgnoreList",
            "shotgun,gunammo,gift,pickle,airhorn",
            "[HOST] A comma separated list of items to ignore in the ship. Does not have to be the exact name but at least a matching portion. e.g. 'trag' for 'tragedy'"
        ).Value;

        ShowDebugLogs = configFile.Bind(
            "Logs",
            "ShowDebugLogs",
            false,
            "[CLIENT] Turn on/off debug logs."
        ).Value;
    }
}
