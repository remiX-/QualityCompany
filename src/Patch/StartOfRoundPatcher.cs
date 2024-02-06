using HarmonyLib;
using Newtonsoft.Json;
using QualityCompany.Components;
using QualityCompany.Modules;
using QualityCompany.Network;
using QualityCompany.Service;
using QualityCompany.Utils;
using System.Text;
using TMPro;
using UnityEngine;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatcher
{
    private static readonly ACLogger _logger = new(nameof(StartOfRoundPatcher));

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void Initialize()
    {
        GameUtils.Init();

        // TODO see if better place?
        var moduleLoaderGameObject = new GameObject("QualityCompanyLoader");
        moduleLoaderGameObject.AddComponent<ModuleLoader>();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ReviveDeadPlayers")]
    private static void PlayerHasRevivedServerRpc()
    {
        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SyncShipUnlockablesClientRpc")]
    private static void RefreshLootForClientOnStart()
    {
        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ChangeLevelClientRpc")]
    private static void SwitchPlanets()
    {
        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("StartGame")]
    private static void StartGame()
    {
        OvertimeMonitor.UpdateMonitor();

        _logger.LogDebug(JsonConvert.SerializeObject(CompanyNetworkHandler.Instance.SaveData));
        Plugin.Instance.PluginConfig.DebugPrintConfig(_logger);
    }

    [HarmonyPostfix]
    [HarmonyPatch("ArriveAtLevel")]
    private static void ArriveAtLevel()
    {
        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
    private static void ColorWeather(ref TextMeshProUGUI ___screenLevelDescription, ref SelectableLevel ___currentLevel)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("Orbiting: " + ___currentLevel.PlanetName + "\n");
        stringBuilder.Append("Weather: " + FormatWeather(___currentLevel.currentWeather) + "\n");
        stringBuilder.Append(___currentLevel.LevelDescription ?? "");
        ___screenLevelDescription.text = stringBuilder.ToString();
    }

    private static string FormatWeather(LevelWeatherType currentWeather)
    {
        var colour = currentWeather switch
        {
            LevelWeatherType.None => "69FF6B",
            LevelWeatherType.DustClouds => "69FF6B",
            LevelWeatherType.Rainy => "FFDC00",
            LevelWeatherType.Foggy => "FFDC00",
            LevelWeatherType.Stormy => "FF9300",
            LevelWeatherType.Flooded => "FF9300",
            LevelWeatherType.Eclipsed => "FF0000",
            _ => "FFFFFF"
        };

        return $"<color=#{colour}>{currentWeather}</color>";
    }
}