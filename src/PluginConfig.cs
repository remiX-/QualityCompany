using BepInEx.Configuration;
using Newtonsoft.Json;
using System;

namespace QualityCompany;

[Serializable]
internal class PluginConfig
{
    public string SellIgnoreList { get; set; }

    public bool TerminalMiscCommandsEnabled { get; set; }

    public bool TerminalSellCommandsEnabled { get; set; }

    public bool TerminalTargetCommandsEnabled { get; set; }

    public bool TerminalDebugCommandsEnabled { get; set; }

    [JsonIgnore]
    public bool MonitorLootCreditsEnabled { get; set; }

    [JsonIgnore]
    public bool MonitorInfoEnabled { get; set; }

    [JsonIgnore]
    public bool MonitorTimeEnabled { get; set; }

    [JsonIgnore]
    public bool HUDShowScrapUI { get; set; }

    [JsonIgnore]
    public bool HUDShowShotgunAmmoCounterUI { get; set; }

    [JsonIgnore]
    public bool ShowDebugLogs { get; set; }

    public PluginConfig()
    { }

    public void Bind(ConfigFile configFile)
    {
        #region Terminal
        SellIgnoreList = configFile.Bind(
            "Terminal",
            "SellIgnoreList",
            "shotgun,gunammo,gift",
            "[HOST] A comma separated list of items to ignore in the ship. Does not have to be the exact name but at least a matching portion. e.g. 'trag' for 'tragedy'"
        ).Value;

        TerminalMiscCommandsEnabled = configFile.Bind(
            "Terminal",
            "MiscCommandsEnabled",
            true,
            "[HOST] Turn on/off the additional misc terminal commands. This includes: launch, door, lights, tp, time"
        ).Value;

        TerminalSellCommandsEnabled = configFile.Bind(
            "Terminal",
            "SellCommandsEnabled",
            true,
            "[HOST] Turn on/off the additional 'sell <command>' terminal commands. This includes: all, quota, target, 2h, <amount>, <item>. NOTE: The 'target' sub command will be disabled if TerminalTargetCommandsEnabled is disabled."
        ).Value;

        TerminalTargetCommandsEnabled = configFile.Bind(
            "Terminal",
            "TargetCommandsEnabled",
            true,
            "[HOST] Turn on/off the additional 'target' terminal command."
        ).Value;

        TerminalDebugCommandsEnabled = configFile.Bind(
            "Terminal",
            "DebugCommandsEnabled",
            true,
            "[HOST] Turn on/off the additional 'hack' terminal command. This allows to spawn <amount> of items at your foot. NOTE: This is primary for mod testing purposes, but may come in use ;)"
        ).Value;
        #endregion

        #region Monitor
        MonitorLootCreditsEnabled = configFile.Bind(
            "Monitor",
            "LootCreditsEnabled",
            true,
            "[CLIENT] Turn on/off the ship loot & game credit balance monitor in the ship."
        ).Value;

        MonitorInfoEnabled = configFile.Bind(
            "Monitor",
            "Enabled",
            true,
            "[CLIENT] Turn on/off the info monitor in the ship."
        ).Value;

        MonitorTimeEnabled = configFile.Bind(
            "Monitor",
            "TimeEnabled",
            true,
            "[CLIENT] Turn on/off the time monitor in the ship."
        ).Value;
        #endregion

        #region HUD
        HUDShowScrapUI = configFile.Bind(
            "HUD",
            "ShowScrapUI",
            true,
            "[CLIENT] Turn on/off scrap value on the item slots UI."
        ).Value;

        HUDShowShotgunAmmoCounterUI = configFile.Bind(
            "HUD",
            "ShowShotgunAmmoCounterUI",
            true,
            "[CLIENT] Turn on/off shotgun ammo counter on the item slots UI."
        ).Value;
        #endregion

        #region Terminal
        ShowDebugLogs = configFile.Bind(
            "Debug",
            "ShowDebugLogs",
            false,
            "[CLIENT] Turn on/off debug logs."
        ).Value;
        #endregion
    }
}
