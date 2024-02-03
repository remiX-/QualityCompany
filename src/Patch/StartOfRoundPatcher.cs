using AdvancedCompany.Components;
using AdvancedCompany.Modules;
using AdvancedCompany.Service;
using AdvancedCompany.Utils;
using HarmonyLib;
using System.Text;
using TMPro;
using UnityEngine;

namespace AdvancedCompany.Patch;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatcher
{
    // private static bool hasInitialized;

    private static readonly ACLogger _logger = new(nameof(StartOfRoundPatcher));

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void Initialize(StartOfRound __instance)
    {
        _logger.LogDebug("StartOfRound.Start");
        GameUtils.Init();
        InitializeMonitorCluster();

        // TODO see if better place
        _logger.LogMessage($"Loading ModuleLoader...");
        var mlgo = new GameObject("AdvancedCompanyLoader");
        mlgo.AddComponent<ModuleLoader>();
        // Object.DontDestroyOnLoad(mlgo);

        // hasInitialized = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("ReviveDeadPlayers")]
    private static void PlayerHasRevivedServerRpc()
    {
        LootMonitor.UpdateMonitor();
        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SyncShipUnlockablesClientRpc")]
    private static void RefreshLootForClientOnStart()
    {
        _logger.LogMessage("SyncShipUnlockablesClientRpc");

        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ChangeLevelClientRpc")]
    private static void SwitchPlanets()
    {
        _logger.LogMessage("ChangeLevelClientRpc");

        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("StartGame")]
    private static void StartGame()
    {
        _logger.LogMessage("StartGame");

        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ArriveAtLevel")]
    private static void ArriveAtLevel()
    {
        _logger.LogDebug("ArriveAtLevel");
        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
    private static void ColorWeather(ref TextMeshProUGUI ___screenLevelDescription, ref SelectableLevel ___currentLevel)
    {
        //IL_002b: Unknown result type (might be due to invalid IL or missing references)
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

    private static void InitializeMonitorCluster()
    {
        // if (hasInitialized) return;

        _logger.LogMessage($"Ship: {GameUtils.ShipGameObject}");

        var hangerShipMainContainer = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer");
        var hangerShipHeaderText = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText");
        Object.Destroy(GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/BG"));
        Object.Destroy(GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/BG (1)"));

        // LootMonitor
        var lootMonitor = new GameObject("lootMonitor")
        {
            name = "lootMonitorText",
            transform =
            {
                parent = hangerShipMainContainer.transform,
                position = hangerShipMainContainer.transform.position,
                localPosition = hangerShipMainContainer.transform.localPosition,
                localScale = Vector3.one,
                rotation = Quaternion.Euler(Vector3.zero)
            }
        };
        var lootMonitorText = Object.Instantiate(hangerShipHeaderText, lootMonitor.transform);
        lootMonitorText.name = "lootMonitorText";
        lootMonitorText.transform.localPosition = new Vector3(-95f, 450f, 220f);
        lootMonitorText.transform.rotation = Quaternion.Euler(new Vector3(-20f, 90f, 0f));
        lootMonitorText.AddComponent<LootMonitor>();

        // OvertimeMonitor
        var overtimeMonitor = new GameObject("overtimeMonitor")
        {
            transform =
            {
                parent = hangerShipMainContainer.transform,
                position = hangerShipMainContainer.transform.position,
                localPosition = hangerShipMainContainer.transform.localPosition,
                localScale = Vector3.one,
                rotation = Quaternion.Euler(Vector3.zero)
            }
        };
        var overtimeMonitorText = Object.Instantiate(hangerShipHeaderText, overtimeMonitor.transform);
        overtimeMonitorText.name = "overtimeMonitorText";
        overtimeMonitorText.transform.localPosition = new Vector3(-95f, 450f, -250f);
        overtimeMonitorText.transform.rotation = Quaternion.Euler(new Vector3(-20f, 90f, 0f));
        overtimeMonitorText.AddComponent<OvertimeMonitor>();

        // TimeMonitor
        var timeMonitor = new GameObject("timeMonitor")
        {
            transform =
            {
                parent = hangerShipMainContainer.transform,
                position = hangerShipMainContainer.transform.position,
                localPosition = hangerShipMainContainer.transform.localPosition,
                localScale = Vector3.one,
                rotation = Quaternion.Euler(Vector3.zero)
            }
        };
        var timeMonitorText = Object.Instantiate(hangerShipHeaderText, timeMonitor.transform);
        timeMonitorText.name = "timeMonitorText";
        timeMonitorText.transform.localPosition = new Vector3(-413f, 450f, -1185f);
        timeMonitorText.transform.rotation = Quaternion.Euler(new Vector3(-21f, 117f, 0f));
        timeMonitorText.AddComponent<TimeMonitor>();

        // CreditMonitor
        var creditMonitor = new GameObject("creditMonitor")
        {
            transform =
            {
                parent = hangerShipMainContainer.transform,
                position = hangerShipMainContainer.transform.position,
                localPosition = hangerShipMainContainer.transform.localPosition,
                localScale = Vector3.one,
                rotation = Quaternion.Euler(Vector3.zero)
            }
        };
        var creditMonitorText = Object.Instantiate(hangerShipHeaderText, creditMonitor.transform);
        creditMonitorText.name = "creditMonitorText";
        creditMonitorText.transform.localPosition = new Vector3(-198f, 450f, -750f);
        creditMonitorText.transform.rotation = Quaternion.Euler(new Vector3(-21f, 117f, 0f));
        creditMonitorText.AddComponent<CreditMonitor>();
    }
}