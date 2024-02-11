using QualityCompany.Modules.HUD;
using QualityCompany.Service;
using System;
using Unity.Netcode;

namespace QualityCompany.Network;

internal class LatencyHandler : NetworkBehaviour
{
    public static LatencyHandler Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(LatencyHandler));

    [ServerRpc(RequireOwnership = false)]
    internal void PingServerRpc()
    {
        _logger.LogDebug($"PingServerRpc: {DateTime.Now:HH:mm:ss.fff}");
        PingClientRpc();
    }

    [ClientRpc]
    private void PingClientRpc()
    {
        if (NetworkManager.IsHost) return;

        _logger.LogDebug($"PingClientRpc: {DateTime.Now:HH:mm:ss.fff}");
        PingModule.Instance.UpdateLatency();
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;

        base.OnNetworkSpawn();
    }
}