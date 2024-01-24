using AdvancedCompany.Network;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace AdvancedCompany;

[HarmonyPatch]
public class NetworkObjectManager
{
    private static GameObject networkPrefab;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameNetworkManager), "Start")]
    public static void Init()
    {
        Logger.LogDebug("NetworkObjectManager.Start");
        if (networkPrefab != null)
        {
            return;
        }

        Logger.LogDebug(" > Loading ExampleNetworkHandler...");
        networkPrefab = Plugin.CustomAssets.LoadAsset<GameObject>("ExampleNetworkHandler");
        Logger.LogDebug($" > done! {networkPrefab == null}");
        networkPrefab.AddComponent<NetworkHandler>();
        Logger.LogDebug($" > added NetworkHandler component");

        NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        Logger.LogDebug($" > added to NetworkManager.Singleton");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    public static void SpawnNetworkHandlerObject()
    {
        Logger.LogDebug("NetworkObjectManager.Awake");
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            var networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
        }
    }
}
