using HarmonyLib;
using Newtonsoft.Json;
using QualityCompany.Manager.Saves;
using QualityCompany.Modules.Core;
using QualityCompany.Modules.Ship;
using QualityCompany.Service;
using QualityCompany.Utils;
using System.Text;
using TMPro;
using UnityEngine;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatcher
{
    private static readonly ModLogger Logger = new(nameof(StartOfRoundPatcher));

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    public static void AwakePatch(StartOfRound __instance)
    {
        OnStartOfRoundAwake(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    public static void StartPatch(StartOfRound __instance)
    {
        GameUtils.Init();

        OnStartOfRoundStart(__instance);

        SaveManager.Load();

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
        OnPlayersFired(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("EndOfGame")]
    private static void EndOfGamePatch(StartOfRound __instance)
    {
        OnEndOfGame(__instance);
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
        InfoMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ArriveAtLevel")]
    private static void ArriveAtLevelPatch()
    {
        InfoMonitor.UpdateMonitor();
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