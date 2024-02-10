using QualityCompany.Modules.Core;
using UnityEngine;

namespace QualityCompany.Modules.Ship;

[Module]
internal class MonitorModule
{
    private static readonly (Vector3 Position, Vector3 Rotation)[] monitorLocations = {
        (new Vector3(-95f, 450f, 220f), new Vector3(-20f, 90f, 0f)),
        (new Vector3(-95f, 450f, -250f), new Vector3(-20f, 90f, 0f)),
        (new Vector3(-198f, 450f, -750f), new Vector3(-21f, 117f, 0f)),
        (new Vector3(-413f, 450f, -1185f), new Vector3(-21f, 117f, 0f))
    };

    private static GameObject hangerShipMainContainer;
    private static GameObject hangerShipHeaderText;

    [ModuleOnLoad]
    private static void Handle()
    {
        hangerShipMainContainer = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer");
        hangerShipHeaderText = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText");
        Object.Destroy(GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/BG"));
        Object.Destroy(GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/BG (1)"));

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

    private static void InitMonitor<T>(ref int currentMonitorIndex) where T : Component
    {
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
        var monitorText = Object.Instantiate(hangerShipHeaderText, monitorGO.transform);
        monitorText.name = $"qc_monitor_{currentMonitorIndex}Text";
        monitorText.transform.localPosition = position;
        monitorText.transform.rotation = Quaternion.Euler(rotation);
        monitorText.AddComponent<T>();

        currentMonitorIndex += 1;
    }
}