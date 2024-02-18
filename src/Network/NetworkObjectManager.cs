using HarmonyLib;
using QualityCompany.Assets;
using QualityCompany.Service;
using Unity.Netcode;
using UnityEngine;

namespace QualityCompany.Network;

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
        if (!Plugin.Instance.PluginConfig.NetworkingEnabled) return;
        if (networkPrefab != null || hasInit) return;

        hasInit = true;

        networkPrefab = AssetManager.CustomAssets.LoadAsset<GameObject>("QualityCompanyNetworkHandler");
        networkPrefab.AddComponent<NetworkHandler>();
        networkPrefab.AddComponent<CompanyNetworkHandler>();
        networkPrefab.AddComponent<LatencyHandler>();

        NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    public static void SpawnNetworkHandlerObject()
    {
        if (!Plugin.Instance.PluginConfig.NetworkingEnabled) return;
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer) return;

        var networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }
}
