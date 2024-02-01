using AdvancedCompany.Components;
using AdvancedCompany.Modules;
using AdvancedCompany.Network;
using AdvancedCompany.Service;
using AdvancedCompany.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using static AdvancedCompany.Service.GameEvents;

namespace AdvancedCompany.Patch;

internal class MonitorPatch
{
    public static float overTimeMonitorX = -95f;
    public static float overTimeMonitorY = 450f;

    private static bool hasInitialized = false;

    private static readonly ACLogger _logger = new(nameof(MonitorPatch));

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    private static void Initialize()
    {
        _logger.LogDebug("StartOfRound.Start");
        InitializeMonitorCluster();

        GameUtils.Init();

        hasInitialized = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HUDManager), "Start")]
    private static void HudManagerStartPatch(HUDManager __instance)
    {
        _logger.LogDebug("HudManagerStartPatch");

        InitializeMonitorCluster();

        GameUtils.Init();

        // TODO: move shotty ammo ui loading to a ModuleLoader of sorts
        var shotty = new GameObject("ShotgunAmmoUI");
        shotty.AddComponent<ShotgunUIModule>();
        shotty.transform.parent = __instance.HUDContainer.transform;

        hasInitialized = true;

        OnHudManagerStart(__instance);
    }

    private static void InitializeMonitorCluster()
    {
        if (hasInitialized) return;

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
        _logger.LogDebug("PlaceItemOnCounter");

        // OvertimeMonitor.UpdateMonitor();
        CompanyNetworkHandler.Instance.SyncDepositDeskTotalValueServerRpc();
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.IsHost}");
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.name}");
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.playerSteamId}");
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.playerUsername}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DepositItemsDesk), "AddObjectToDeskClientRpc")]
    private static void CalculateTotalOnDesk()
    {
        _logger.LogDebug("AddObjectToDeskClientRpc");

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
    private static void OvertimeBonus(TimeOfDay __instance)
    {
        _logger.LogMessage("SyncNewProfitQuotaClientRpc");

        CompanyNetworkHandler.Instance.SaveData.TotalLootValue = ScrapUtils.GetShipTotalRawScrapValue();
        CompanyNetworkHandler.Instance.SaveData.TotalDaysPlayedForCurrentQuota = 0;

        // if (GameNetworkManager.Instance.localPlayerController.IsHost)
        if (__instance.IsHost)
        {
            // update save file
            CompanyNetworkHandler.Instance.ServerSaveFileServerRpc();
        }

        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "SyncShipUnlockablesClientRpc")]
    private static void RefreshLootForClientOnStart()
    {
        _logger.LogMessage("SyncShipUnlockablesClientRpc");

        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
    private static void OnPlayerConnect()
    {
        _logger.LogMessage("ConnectClientToPlayerObject");

        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TimeOfDay), "MoveTimeOfDay")]
    private static void RefreshClock()
    {
        TimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), "BeginGrabObject")]
    private static void BeginGrabObjectPatch(PlayerControllerB __instance)
    {
        OnPlayerBeginGrabObject(__instance);
    }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(ShotgunItem), "reloadGunAnimation")]
    // private static void reloadGunAnimationPatch()
    // {
    //     OnPlayerBeginGrabObject(GameNetworkManager.Instance.localPlayerController);
    // }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), "SwitchToItemSlot")]
    private static void SwitchToItemSlotPatch(PlayerControllerB __instance)
    {
        OnPlayerSwitchToItemSlot(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), "GrabObjectClientRpc")]
    private static void RefreshLootOnPickupClient(bool grabValidated, ref NetworkObjectReference grabbedObject)
    {
        _logger.LogDebug("GrabObjectClientRpc");

        if (!grabbedObject.TryGet(out var networkObject)) return;

        var componentInChildren = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        if (componentInChildren.isInShipRoom || componentInChildren.isInElevator)
        {
            LootMonitor.UpdateMonitor();
            OvertimeMonitor.UpdateMonitor();
        }

        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), "ThrowObjectClientRpc")]
    private static void RefreshLootOnThrowClient(bool droppedInElevator, bool droppedInShipRoom, Vector3 targetFloorPosition, NetworkObjectReference grabbedObject)
    {
        _logger.LogDebug("ThrowObjectClientRpc");

        _logger.LogDebug($" > isInShipRoom: {droppedInShipRoom} | isInElevator: {droppedInElevator}");
        if (droppedInShipRoom || droppedInElevator)
        {
            LootMonitor.UpdateMonitor();
            OvertimeMonitor.UpdateMonitor();
        }

        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "ChangeLevelClientRpc")]
    private static void SwitchPlanets()
    {
        _logger.LogMessage("ChangeLevelClientRpc");

        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Terminal), "SyncGroupCreditsClientRpc")]
    private static void RefreshMoney()
    {
        _logger.LogMessage("SyncGroupCreditsClientRpc");

        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "StartGame")]
    private static void StartGame()
    {
        _logger.LogMessage("StartGame");

        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "ArriveAtLevel")]
    private static void ArriveAtLevel()
    {
        _logger.LogDebug("ArriveAtLevel");
        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TimeOfDay), "UpdateProfitQuotaCurrentTime")]
    private static void UpdateProfitQuotaCurrentTime()
    {
        if (!hasInitialized) return;

        OvertimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HUDManager), "DisplayDaysLeft")]
    private static void DisplayDaysLeft(TimeOfDay __instance)
    {
        _logger.LogDebug($"UpdateProfitQuotaCurrentTime: {hasInitialized}");
        if (!hasInitialized) return;

        CompanyNetworkHandler.Instance.SaveData.TotalDaysPlayedForCurrentQuota++;

        // if (GameNetworkManager.Instance.localPlayerController.IsHost)
        if (__instance.IsHost)
        {
            // update save file
            CompanyNetworkHandler.Instance.ServerSaveFileServerRpc();
        }

        OvertimeMonitor.UpdateMonitor();
    }
}