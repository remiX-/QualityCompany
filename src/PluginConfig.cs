using BepInEx.Configuration;
using Newtonsoft.Json;
using QualityCompany.Service;
using System;
using System.ComponentModel;

namespace QualityCompany;

[Serializable]
internal class PluginConfig
{
    public string SellIgnoreList { get; set; }

    public bool TerminalMiscCommandsEnabled { get; set; }

    public bool TerminalSellCommandsEnabled { get; set; }

    public bool TerminalTargetCommandsEnabled { get; set; }

    public bool TerminalDebugCommandsEnabled { get; set; }

    public bool TerminalPatchFixScanEnabled { get; set; }

    [JsonIgnore]
    public bool MonitorLootCreditsEnabled { get; set; }

    [JsonIgnore]
    public bool MonitorInfoEnabled { get; set; }

    [JsonIgnore]
    public bool MonitorTimeEnabled { get; set; }

    [JsonIgnore]
    public bool InventoryShowScrapUI { get; set; }

    [JsonIgnore]
    public bool InventoryShowShotgunAmmoCounterUI { get; set; }

    [JsonIgnore]
    public bool InventoryForceUpdateAllSlotsOnDiscard { get; set; }

    [JsonIgnore]
    public float InventoryStartupDelay { get; set; }

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

        #region Inventory
        InventoryShowScrapUI = configFile.Bind(
            "HUD",
            "ShowScrapUI",
            true,
            "[CLIENT] Turn on/off scrap value on the item slots UI."
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
        #endregion

        #region Debug
        ShowDebugLogs = configFile.Bind(
            "Debug",
            "ShowDebugLogs",
            false,
            "[CLIENT] Turn on/off debug logs."
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
    }

    public void DebugPrintConfig(ACLogger logger)
    {
        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
        {
            var name = descriptor.Name;
            var value = descriptor.GetValue(this);
            logger.LogDebug($"{name}={value}");
        }
    }
}
