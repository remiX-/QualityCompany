using GameNetcodeStuff;
using HarmonyLib;
using AdvancedCompany.Components;
using Unity.Netcode;
using UnityEngine;

namespace AdvancedCompany.Patch;

internal class MonitorPatch
{
    public static float overTimeMonitorX = -95f;
    public static float overTimeMonitorY = 450f;

    private static bool hasInitialized = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    private static void Initialize()
    {
        InitializeMonitorCluster();
        hasInitialized = true;
    }

    private static void InitializeMonitorCluster()
    {
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
        lootMonitorText.transform.localPosition = new Vector3(overTimeMonitorX, overTimeMonitorY, 220f);
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "ReviveDeadPlayers")]
    private static void PlayerHasRevivedServerRpc()
    {
        LootMonitor.UpdateMonitor();
        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HUDManager), "ApplyPenalty")]
    private static void ApplyPenalty()
    {
        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DepositItemsDesk), "PlaceItemOnCounter")]
    private static void PlaceItemOnCounterPatch(ref PlayerControllerB playerWhoTriggered)
    {
        OvertimeMonitor.UpdateMonitor();
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.IsHost}");
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.name}");
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.playerSteamId}");
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.playerUsername}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DepositItemsDesk), "AddObjectToDeskClientRpc")]
    private static void CalculateTotalOnDesk()
    {
        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HUDManager), "DisplayCreditsEarning")]
    private static void SellLoot()
    {
        OvertimeMonitor.UpdateMonitor();
        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TimeOfDay), "SyncNewProfitQuotaClientRpc")]
    private static void OvertimeBonus()
    {
        Logger.LogMessage("SyncNewProfitQuotaClientRpc");
        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "SyncShipUnlockablesClientRpc")]
    private static void RefreshLootForClientOnStart()
    {
        Logger.LogMessage("SyncShipUnlockablesClientRpc");
        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
    private static void OnPlayerConnect()
    {
        Logger.LogMessage("ConnectClientToPlayerObject");
        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TimeOfDay), "MoveTimeOfDay")]
    private static void RefreshClock()
    {
        TimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), "GrabObjectClientRpc")]
    private static void RefreshLootOnPickupClient(bool grabValidated, ref NetworkObjectReference grabbedObject)
    {
        if (!grabbedObject.TryGet(out var networkObject)) return;

        var componentInChildren = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        if (componentInChildren.isInShipRoom | componentInChildren.isInElevator)
        {
            LootMonitor.UpdateMonitor();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), "ThrowObjectClientRpc")]
    private static void RefreshLootOnThrowClient(bool droppedInElevator, bool droppedInShipRoom, Vector3 targetFloorPosition, NetworkObjectReference grabbedObject)
    {
        if (droppedInShipRoom || droppedInElevator)
        {
            LootMonitor.UpdateMonitor();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "ChangeLevelClientRpc")]
    private static void SwitchPlanets()
    {
        Logger.LogMessage("ChangeLevelClientRpc");
        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Terminal), "SyncGroupCreditsClientRpc")]
    private static void RefreshMoney()
    {
        Logger.LogMessage("SyncGroupCreditsClientRpc");
        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "StartGame")]
    private static void StartGame()
    {
        Logger.LogMessage("StartGame");

        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "ArriveAtLevel")]
    private static void ArriveAtLevel()
    {
        Logger.LogMessage("ArriveAtLevel");
        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TimeOfDay), "UpdateProfitQuotaCurrentTime")]
    private static void UpdateProfitQuotaCurrentTime()
    {
        Logger.LogMessage("UpdateProfitQuotaCurrentTime");
        if (!hasInitialized) return;
        OvertimeMonitor.UpdateMonitor();
    }
}