using AdvancedCompany.Service;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace AdvancedCompany.Network;

[HarmonyPatch]
internal class NetworkObjectManager
{
    private static readonly ACLogger _logger = new(nameof(NetworkObjectManager));

    private static GameObject networkPrefab;

    private static bool hasInit;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameNetworkManager), "Start")]
    public static void Init()
    {
        _logger.LogDebug($"NOM: {hasInit}");
        if (networkPrefab != null || hasInit)
        {
            return;
        }

        hasInit = true;

        networkPrefab = Plugin.CustomAssets.LoadAsset<GameObject>("ExampleNetworkHandler");
        networkPrefab.AddComponent<NetworkHandler>();
        networkPrefab.AddComponent<CompanyNetworkHandler>();

        NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    public static void SpawnNetworkHandlerObject()
    {
        _logger.LogDebug($"~~~ SpawnNetworkHandlerObject | {NetworkManager.Singleton.IsHost} | {NetworkManager.Singleton.IsServer}");
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            _logger.LogDebug("networkPrefab null? " + networkPrefab == null);
            var networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
        }
    }
}
