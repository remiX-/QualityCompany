using BepInEx.Configuration;
using System;

namespace AdvancedCompany;

[Serializable]
internal class PluginConfig
{
    readonly ConfigFile configFile;

    public string ConfigSellIgnoreList { get; set; }

    public PluginConfig(ConfigFile config)
    {
        configFile = config;
    }

    public void Bind()
    {
        ConfigSellIgnoreList = configFile.Bind(
            "Selling",
            "SellIgnoreList",
            "shotgun,gunammo,gift,pickle,airhorn",
            "A comma separated list of items to ignore in the ship. Does not have to be the exact name but at least a matching portion. e.g. 'trag' for 'tragedy'"
        ).Value;
    }
}
