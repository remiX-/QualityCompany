using BepInEx.Configuration;
using Newtonsoft.Json;
using QualityCompany.Service;
using System;
using System.ComponentModel;

namespace QualityCompany;

[Serializable]
internal class PluginConfig
{
    #region Debug
    [JsonIgnore]
    public bool ShowDebugLogs { get; set; }

    [JsonIgnore]
    public bool ExperimentalFeaturesEnabled { get; set; }
    #endregion

    #region HUD
    [JsonIgnore]
    public bool InventoryShowScrapUI { get; set; }

    [JsonIgnore]
    public bool InventoryShowTotalScrapUI { get; set; }

    [JsonIgnore]
    public bool InventoryScrapForceRefresh { get; set; }

    [JsonIgnore]
    public bool InventoryShowShotgunAmmoCounterUI { get; set; }

    [JsonIgnore]
    public bool InventoryForceUpdateAllSlotsOnDiscard { get; set; }

    [JsonIgnore]
    public float InventoryStartupDelay { get; set; }

    [JsonIgnore]
    public bool HudLatencyEnabled { get; set; }

    [JsonIgnore]
    public float HudLatencyUpdateInterval { get; set; }

    [JsonIgnore]
    public string HudLatencyAnchor { get; set; }

    [JsonIgnore]
    public float HudLatencyHorizontalPadding { get; set; }

    [JsonIgnore]
    public float HudLatencyVerticalPadding { get; set; }
    #endregion

    #region Monitor
    [JsonIgnore]
    public bool MonitorLootCreditsEnabled { get; set; }

    [JsonIgnore]
    public bool MonitorInfoEnabled { get; set; }

    [JsonIgnore]
    public bool MonitorTimeEnabled { get; set; }
    #endregion

    #region Networking

    [JsonIgnore]
    public bool NetworkingEnabled { get; set; }
    #endregion

    #region Terminal
    public string SellIgnoreList { get; set; }

    public bool TerminalMiscCommandsEnabled { get; set; }

    public bool TerminalSellCommandsEnabled { get; set; }

    public bool TerminalTargetCommandsEnabled { get; set; }

    public bool TerminalDebugCommandsEnabled { get; set; }

    public bool TerminalPatchFixScanEnabled { get; set; }

    public bool TerminalPatchFixScanItemsListEnabled { get; set; }
    #endregion

    public PluginConfig()
    { }

    public void Bind(ConfigFile configFile)
    {
        #region Debug
        ShowDebugLogs = configFile.Bind(
            "Debug",
            "ShowDebugLogs",
            false,
            "[CLIENT] Turn on/off debug logs."
        ).Value;

        ExperimentalFeaturesEnabled = configFile.Bind(
            "Debug",
            "ExperimentalFeaturesEnabled",
            false,
            "[CLIENT] Turn on/off experimental features."
        ).Value;
        #endregion

        #region HUD
        InventoryShowScrapUI = configFile.Bind(
            "HUD",
            "ShowScrapUI",
            true,
            "[CLIENT] Turn on/off scrap value on the item slots UI."
        ).Value;

        InventoryShowTotalScrapUI = configFile.Bind(
            "HUD",
            "ShowTotalScrapUI",
            true,
            "[CLIENT] Turn on/off total held items scrap value UI."
        ).Value;

        InventoryScrapForceRefresh = configFile.Bind(
            "HUD",
            "ScrapForceRefresh",
            false,
            "[CLIENT] Turn on/off force refreshing the Scrap UI every second."
        ).Value;

        InventoryShowShotgunAmmoCounterUI = configFile.Bind(
            "HUD",
            "ShowShotgunAmmoCounterUI",
            true,
            "[CLIENT] Turn on/off shotgun ammo counter on the item slots UI."
        ).Value;

        InventoryForceUpdateAllSlotsOnDiscard = configFile.Bind(
            "HUD",
            "ForceUpdateAllSlotsOnDiscard",
            false,
            "[CLIENT] Turn on/off force updating all item slots scrap & shotgun ui on discarding of a held item."
        ).Value;

        InventoryStartupDelay = configFile.Bind(
            "HUD",
            "StartupDelay",
            4.5f,
            "[CLIENT] Delay before creating inventory UI components for scrap value & shotgun ammo. Minimum value will be set to 3 seconds.\nNOTE: Useful if you have mod compatibility issues with mods that affect the players' inventory slots such as HotBarPlus, GeneralImprovements, ReservedItemSlot (Flashlight, Weapon, etc)"
        ).Value;

        HudLatencyEnabled = configFile.Bind(
            "HUD",
            "Show latency to host",
            true,
            "[CLIENT] Whether to show the latency HUD or not. Disabled for the host by default. Requires networking."
        ).Value;

        HudLatencyUpdateInterval = configFile.Bind(
            "HUD",
            "Latency Update Interval",
            5f,
            "[CLIENT] How often to do latency update checks. Will be set to a minimum of 2 seconds."
        ).Value;

        HudLatencyAnchor = configFile.Bind(
            "HUD",
            "Latency Anchor Position",
            "BottomLeft",
            "[CLIENT] Anchor position to place the latency display.\nPossible values: TopLeft, TopRight, BottomLeft, BottomRight"
        ).Value;

        HudLatencyHorizontalPadding = configFile.Bind(
            "HUD",
            "Latency Horizontal Padding",
            5,
            "[CLIENT] Horizontal padding for the latency hud display away from the horizontal (left/right) edge of the screen."
        ).Value;

        HudLatencyVerticalPadding = configFile.Bind(
            "HUD",
            "Latency Vertical Padding",
            5,
            "[CLIENT] Vertical padding for the latency hud display away from the vertical (top/bottom) edge of the screen."
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
            "InfoEnabled",
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

        #region Networking
        NetworkingEnabled = configFile.Bind(
            "Networking",
            "NetworkingEnabled",
            true,
            "[EXPERIMENTAL!!!] [CLIENT] Turn on/off networking capabilities.\nNOTE: This will MOST LIKELY cause de-sync issues with a couple of things, primarily for non-host clients."
        ).Value;
        #endregion

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
            "[HOST] Turn on/off the additional 'sell <command>' terminal commands. This includes: all, quota, target, 2h, <amount>, <item>.\nNOTE: The 'target' sub command will be disabled if TargetCommandsEnabled is disabled."
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
            "[HOST] Turn on/off the additional 'hack' terminal command. This allows to spawn <amount> of items at your foot.\nNOTE: This is primary for mod testing purposes, but may come in use ;)"
        ).Value;

        TerminalPatchFixScanEnabled = configFile.Bind(
            "Terminal",
            "PatchFixScanEnabled",
            true,
            "[HOST] Turn on/off patch fixing the games' 'scan' command where it occasionally does not work."
        ).Value;

        TerminalPatchFixScanItemsListEnabled = configFile.Bind(
            "Terminal",
            "PatchFixScanItemsListEnabled",
            false,
            "[HOST] Turn on/off scan command showing list of scrap on the moon."
        ).Value;
        #endregion
    }

    public void ApplyHostConfig(PluginConfig hostConfig)
    {
        SellIgnoreList = hostConfig.SellIgnoreList;
        TerminalMiscCommandsEnabled = hostConfig.TerminalMiscCommandsEnabled;
        TerminalSellCommandsEnabled = hostConfig.TerminalSellCommandsEnabled;
        TerminalTargetCommandsEnabled = hostConfig.TerminalTargetCommandsEnabled;
        TerminalDebugCommandsEnabled = hostConfig.TerminalDebugCommandsEnabled;
        TerminalPatchFixScanEnabled = hostConfig.TerminalPatchFixScanEnabled;
        TerminalPatchFixScanItemsListEnabled = hostConfig.TerminalPatchFixScanItemsListEnabled;
    }

    public void DebugPrintConfig(ModLogger logger)
    {
        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
        {
            var name = descriptor.Name;
            var value = descriptor.GetValue(this);
            logger.LogDebug($"{name}={value}");
        }
    }
}
