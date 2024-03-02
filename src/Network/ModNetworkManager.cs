using QualityCompany.Assets;
using Unity.Netcode;
using UnityEngine;
using static QualityCompany.Events.GameEvents;

namespace QualityCompany.Network;

internal class ModNetworkManager
{
    private static GameObject? _networkPrefab;

    private static bool hasInit;

    internal static void Init()
    {
        GameNetworkManagerStart += _ => Start();
        StartOfRoundAwake += _ => Load();
    }

    private static void Start()
    {
        if (!Plugin.Instance.PluginConfig.NetworkingEnabled) return;
        if (_networkPrefab is not null || hasInit) return;

        hasInit = true;

        _networkPrefab = AssetManager.CustomAssets.LoadAsset<GameObject>("ExampleNetworkHandler");
        _networkPrefab.AddComponent<NetworkHandler>();
        _networkPrefab.AddComponent<CompanyNetworkHandler>();
        _networkPrefab.AddComponent<LatencyHandler>();

        NetworkManager.Singleton.AddNetworkPrefab(_networkPrefab);
    }

    private static void Load()
    {
        if (!Plugin.Instance.PluginConfig.NetworkingEnabled) return;
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer) return;
        if (_networkPrefab is null) return;

        var networkHandlerHost = Object.Instantiate(_networkPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }
}
