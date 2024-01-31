using AdvancedCompany.Utils;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace AdvancedCompany.Network;

[HarmonyPatch]
internal class NetworkObjectManager
{
    private static GameObject networkPrefab;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameNetworkManager), "Start")]
    public static void Init()
    {
        if (networkPrefab != null)
        {
            return;
        }

        networkPrefab = Plugin.CustomAssets.LoadAsset<GameObject>("ExampleNetworkHandler");
        networkPrefab.AddComponent<NetworkHandler>();
        networkPrefab.AddComponent<CompanyNetworkHandler>();

        NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    public static void SpawnNetworkHandlerObject()
    {
        Utils.Logger.LogDebug($"~~~ SpawnNetworkHandlerObject | {NetworkManager.Singleton.IsHost} | {NetworkManager.Singleton.IsServer}");
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            Utils.Logger.LogDebug("networkPrefab null? " + networkPrefab == null);
            var networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
        }
    }
}
