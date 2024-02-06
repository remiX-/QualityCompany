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
        (new Vector3(-198f, 450f, -750f), new Vector3(-21f, 117f, 0f)),
        (new Vector3(-413f, 450f, -1185f), new Vector3(-21f, 117f, 0f))
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
        hangerShipHeaderText = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText");
        Destroy(GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/BG"));
        Destroy(GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/BG (1)"));

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
        var (position, rotation) = monitorLocations[currentMonitorIndex];
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

        currentMonitorIndex += 1;
    }
}