using HarmonyLib;
using QualityCompany.Manager.Saves;
using QualityCompany.Modules.Core;
using QualityCompany.Modules.Ship;
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
    private static void StartPatch()
    {
        SaveManager.Load();
        GameUtils.Init();

        // TODO see if better place?
        var moduleLoaderGameObject = new GameObject("QualityCompanyLoader");
        moduleLoaderGameObject.AddComponent<ModuleLoader>();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ReviveDeadPlayers")]
    private static void ReviveDeadPlayersPatch()
    {
        LootMonitor.UpdateMonitor();
    }

    [HarmonyPrefix]
    [HarmonyPatch("playersFiredGameOver")]
    private static void PlayersFiredGameOverPatch(StartOfRound __instance)
    {
        SaveManager.SaveData.ResetGameState();
        SaveManager.Save();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SyncShipUnlockablesClientRpc")]
    private static void SyncShipUnlockablesClientRpcPatch()
    {
        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ChangeLevelClientRpc")]
    private static void ChangeLevelClientRpcPatch()
    {
        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("StartGame")]
    private static void StartGamePatch()
    {
        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ArriveAtLevel")]
    private static void ArriveAtLevelPatch()
    {
        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
    private static void SetMapScreenInfoToCurrentLevelPatch(ref TextMeshProUGUI ___screenLevelDescription, ref SelectableLevel ___currentLevel)
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