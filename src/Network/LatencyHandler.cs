using QualityCompany.Modules.HUD;
using QualityCompany.Service;
using System;
using Unity.Netcode;

namespace QualityCompany.Network;

internal class LatencyHandler : NetworkBehaviour
{
    public static LatencyHandler Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(LatencyHandler));

    private void Start()
    {
        Instance = this;

        var pc = GameNetworkManager.Instance.localPlayerController;
        _logger.LogDebug($"IDs: {pc.playerSteamId} | {pc.actualClientId} | {pc.playerClientId}");

        // if (NetworkManager.IsHost) return;
        // PingServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    internal void PingServerRpc()
    {
        _logger.LogDebug($"PingServerRpc: {DateTime.Now:HH:mm:ss.zzz}");
        PingClientRpc();
    }

    [ClientRpc]
    private void PingClientRpc()
    {
        _logger.LogDebug($"PingClientRpc: {DateTime.Now:HH:mm:ss.zzz}");
        PingModule.Instance.UpdateLatency();
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;

        base.OnNetworkSpawn();
    }
}