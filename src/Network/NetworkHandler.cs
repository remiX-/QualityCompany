using QualityCompany.Manager;
using QualityCompany.Service;
using QualityCompany.Utils;
using Unity.Netcode;

namespace QualityCompany.Network;

internal class NetworkHandler : NetworkBehaviour
{
    internal static NetworkHandler Instance { get; private set; }

    private readonly ModLogger _logger = new(nameof(NetworkHandler));

    [ClientRpc]
    public void SyncValuesClientRpc(int value, NetworkBehaviourReference netRef)
    {
        _logger.LogMessage("SyncValuesClientRpc");
        netRef.TryGet(out GrabbableObject prop);

        if (prop is null)
        {
            _logger.LogError("Unable to resolve net ref for SyncValuesClientRpc!");
            return;
        }

        prop.transform.parent = GameUtils.ShipGameObject.transform;

        if (value == 0) return;

        prop.scrapValue = value;
        prop.itemProperties.creditsWorth = value;
        prop.GetComponentInChildren<ScanNodeProperties>().subText = $"Value: ${value}";

        _logger.LogInfo($"Successfully synced values of {prop.itemProperties.itemName}");
    }

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