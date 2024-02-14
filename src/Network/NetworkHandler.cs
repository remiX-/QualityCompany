using QualityCompany.Manager;
using QualityCompany.Service;
using Unity.Netcode;

namespace QualityCompany.Network;

internal class NetworkHandler : NetworkBehaviour
{
    internal static NetworkHandler Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(NetworkHandler));

    [ServerRpc(RequireOwnership = false)]
    internal void UpdateSellTargetServerRpc(int newTarget, string playerName)
    {
        UpdateSellTargetClientRpc(newTarget, playerName);
    }

    [ClientRpc]
    internal void UpdateSellTargetClientRpc(int newTarget, string playerName)
    {
        TargetManager.UpdateTargetClient(newTarget, playerName);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SellAllScrapServerRpc()
    {
        SellAllScrapClientRpc();
    }

    [ClientRpc]
    internal void SellAllScrapClientRpc()
    {
        TargetManager.SellAllScrapClient();
    }

    [ServerRpc(RequireOwnership = false)]
    internal void TargetSellForNetworkObjectServerRpc(ulong networkObjectId)
    {
        TargetSellForNetworkObjectClientRpc(networkObjectId);
    }

    [ClientRpc]
    internal void TargetSellForNetworkObjectClientRpc(ulong networkObjectId)
    {
        TargetManager.MoveNetworkObjectToDepositDeskClient(networkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void ExecuteSellAmountServerRpc()
    {
        ExecuteSellAmountClientRpc();
    }

    [ClientRpc]
    internal void ExecuteSellAmountClientRpc()
    {
        TargetManager.ExecuteTargetedSellOrderClient();
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;

        base.OnNetworkSpawn();
    }
}