using QualityCompany.Modules.HUD;
using Unity.Netcode;

namespace QualityCompany.Network;

internal class LatencyHandler : NetworkBehaviour
{
    public static LatencyHandler Instance { get; private set; }

    [ServerRpc(RequireOwnership = false)]
    internal void PingServerRpc(ulong playerClientId)
    {
        PingClientRpc(playerClientId);
    }

    [ClientRpc]
    private void PingClientRpc(ulong playerClientId)
    {
        if (NetworkManager.IsHost) return;
        if (GameNetworkManager.Instance.localPlayerController.playerClientId != playerClientId) return;

        LatencyModule.Instance.UpdateLatency();
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;

        base.OnNetworkSpawn();
    }
}