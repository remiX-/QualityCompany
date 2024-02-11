using QualityCompany.Modules.HUD;
using QualityCompany.Service;
using Unity.Netcode;

namespace QualityCompany.Network;

internal class LatencyHandler : NetworkBehaviour
{
    public static LatencyHandler Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(LatencyHandler));

    private void Start()
    {
        Instance = this;

        if (NetworkManager.IsHost) return;

        var pc = GameNetworkManager.Instance.localPlayerController;
        _logger.LogDebug($"{pc.playerSteamId} | {pc.actualClientId} | {pc.playerClientId}");

        PingServerRpc();
    }

    [ServerRpc(RequireOwnership = true)]
    internal void PingServerRpc()
    {
        PingClientRpc();
    }

    [ClientRpc]
    private void PingClientRpc()
    {
        PingModule.Instance.UpdateLatency();
        // if (retrievedCfg)
        // {
        //     _logger.LogDebug("Config has already been received from host on this client, disregarding.");
        //     return;
        // }
        //
        // var cfg = JsonConvert.DeserializeObject<PluginConfig>(json);
        // if (cfg != null && !IsHost && !IsServer)
        // {
        //     _logger.LogDebug("Config received, deserializing and constructing...");
        //     Plugin.Instance.PluginConfig.ApplyHostConfig(cfg);
        //     retrievedCfg = true;
        // }
        //
        // if (IsHost || IsServer)
        // {
        //     StartCoroutine(WaitALittleToShareTheFile());
        // }
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            // This is recommended in the docs, but it doesn't seem to work
            // https://lethal.wiki/dev/advanced/networking#preventing-duplication
            // Instance?.gameObject?.GetComponent<NetworkObject>()?.Despawn();
        }

        Instance = this;

        base.OnNetworkSpawn();
    }
}