using QualityCompany.Components;
using QualityCompany.Service;
using UnityEngine;

namespace QualityCompany.Modules;

internal class MonitorModule : MonoBehaviour
{
    private readonly ACLogger _logger = new(nameof(MonitorModule));

    private readonly (Vector3 Position, Vector3 Rotation)[] monitorLocations = {
        (new Vector3(-95f, 450f, 220f), new Vector3(-20f, 90f, 0f)),
        (new Vector3(-95f, 450f, -250f), new Vector3(-20f, 90f, 0f)),
        (new Vector3(-413f, 450f, -1185f), new Vector3(-21f, 117f, 0f)),
        (new Vector3(-198f, 450f, -750f), new Vector3(-21f, 117f, 0f)),
    };

    private GameObject hangerShipMainContainer;
    private GameObject hangerShipHeaderText;

    // Maybe some kind of [ModuleOnSpawn] attribute?
    public static void Spawn()
    {
        var scrapUI = new GameObject(nameof(MonitorModule));
        scrapUI.AddComponent<MonitorModule>();
    }

    private void Awake()
    {
        InitializeMonitorCluster();
    }

    private void Start()
    {
        // Destroy(this);
    }

    private void InitializeMonitorCluster()
    {
        hangerShipMainContainer = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer");
        hangerShipHeaderText = GameObject.Find("HeaderText");

        // Destroy(GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/BG"));
        // Destroy(GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/BG (1)"));

        var currentMonitorIndex = 0;
        if (Plugin.Instance.PluginConfig.MonitorLootCreditsEnabled)
        {
            InitMonitor<LootMonitor>(ref currentMonitorIndex);
        }
        if (Plugin.Instance.PluginConfig.MonitorInfoEnabled)
        {
            InitMonitor<OvertimeMonitor>(ref currentMonitorIndex);
        }
        if (Plugin.Instance.PluginConfig.MonitorTimeEnabled)
        {
            InitMonitor<TimeMonitor>(ref currentMonitorIndex);
        }
    }

    private void InitMonitor<T>(ref int currentMonitorIndex) where T : Component
    {
        _logger.LogDebug($"InitMonitor: index {currentMonitorIndex}");
        var (position, rotation) = monitorLocations[currentMonitorIndex++];
        var monitorGO = new GameObject($"qc_monitor_{currentMonitorIndex}")
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
        var monitorText = Instantiate(hangerShipHeaderText, monitorGO.transform);
        monitorText.name = $"qc_monitor_{currentMonitorIndex}Text";
        monitorText.transform.localPosition = position;
        monitorText.transform.rotation = Quaternion.Euler(rotation);
        monitorText.AddComponent<T>();
    }

    // private void InitLootCreditMonitor(ref int currentMonitorIndex)
    // {
    //     if (!Plugin.Instance.PluginConfig.MonitorLootCreditsEnabled) return;
    //
    //     var (position, rotation) = monitorLocations[currentMonitorIndex++];
    //     var lootMonitor = new GameObject("lootMonitor")
    //     {
    //         name = "lootMonitorText",
    //         transform =
    //         {
    //             parent = hangerShipMainContainer.transform,
    //             position = hangerShipMainContainer.transform.position,
    //             localPosition = hangerShipMainContainer.transform.localPosition,
    //             localScale = Vector3.one,
    //             rotation = Quaternion.Euler(Vector3.zero)
    //         }
    //     };
    //     var lootMonitorText = Instantiate(hangerShipHeaderText, lootMonitor.transform);
    //     lootMonitorText.name = "lootMonitorText";
    //     // lootMonitorText.transform.localPosition = new Vector3(-95f, 450f, 220f);
    //     // lootMonitorText.transform.rotation = Quaternion.Euler(new Vector3(-20f, 90f, 0f));
    //     lootMonitorText.transform.localPosition = position;
    //     lootMonitorText.transform.rotation = Quaternion.Euler(rotation);
    //     lootMonitorText.AddComponent<LootMonitor>();
    // }
    //
    // private void InitInfoMonitor(ref int currentMonitorIndex)
    // {
    //     if (!Plugin.Instance.PluginConfig.MonitorLootCreditsEnabled) return;
    //
    //     var (position, rotation) = monitorLocations[currentMonitorIndex++];
    //     var lootMonitor = new GameObject("lootMonitor")
    //     {
    //         name = "lootMonitorText",
    //         transform =
    //         {
    //             parent = hangerShipMainContainer.transform,
    //             position = hangerShipMainContainer.transform.position,
    //             localPosition = hangerShipMainContainer.transform.localPosition,
    //             localScale = Vector3.one,
    //             rotation = Quaternion.Euler(Vector3.zero)
    //         }
    //     };
    //     var lootMonitorText = Instantiate(hangerShipHeaderText, lootMonitor.transform);
    //     lootMonitorText.name = "lootMonitorText";
    //     // lootMonitorText.transform.localPosition = new Vector3(-95f, 450f, 220f);
    //     // lootMonitorText.transform.rotation = Quaternion.Euler(new Vector3(-20f, 90f, 0f));
    //     lootMonitorText.transform.localPosition = position;
    //     lootMonitorText.transform.rotation = Quaternion.Euler(rotation);
    //     lootMonitorText.AddComponent<LootMonitor>();
    // }
    //
    // private void InitTimeMonitor(ref int currentMonitorIndex)
    // {
    //     if (!Plugin.Instance.PluginConfig.MonitorLootCreditsEnabled) return;
    //
    //     var (position, rotation) = monitorLocations[currentMonitorIndex++];
    //
    //     var timeMonitor = new GameObject("timeMonitor")
    //     {
    //         transform =
    //         {
    //             parent = hangerShipMainContainer.transform,
    //             position = hangerShipMainContainer.transform.position,
    //             localPosition = hangerShipMainContainer.transform.localPosition,
    //             localScale = Vector3.one,
    //             rotation = Quaternion.Euler(Vector3.zero)
    //         }
    //     };
    //     var timeMonitorText = Instantiate(hangerShipHeaderText, timeMonitor.transform);
    //     timeMonitorText.name = "timeMonitorText";
    //     timeMonitorText.transform.localPosition = position;
    //     timeMonitorText.transform.rotation = Quaternion.Euler(rotation);
    //     timeMonitorText.AddComponent<TimeMonitor>();
    // }
}

