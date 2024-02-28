using QualityCompany.Manager;
using QualityCompany.Service;
using QualityCompany.Utils;
using Unity.Netcode;

namespace QualityCompany.Network;

internal class NetworkHandler : NetworkBehaviour
{
    internal static NetworkHandler Instance { get; private set; } = null!;

    private readonly ModLogger Logger = new(nameof(NetworkHandler));

    [ClientRpc]
    internal void SyncValuesClientRpc(int value, NetworkBehaviourReference netRef)
    {
        Logger.LogMessage("SyncValuesClientRpc");
        netRef.TryGet(out GrabbableObject prop);

        if (prop is null)
        {
            Logger.LogError("Unable to resolve net ref for SyncValuesClientRpc!");
            return;
        }

        prop.transform.parent = GameUtils.ShipGameObject.transform;

        if (value == 0) return;

        prop.scrapValue = value;
        prop.itemProperties.creditsWorth = value;
        prop.GetComponentInChildren<ScanNodeProperties>().subText = $"Value: ${value}";

        Logger.TryLogDebug($"Successfully synced values of {prop.itemProperties.itemName}");
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
    internal void TargetSellForNetworkObjectsServerRpc(ulong[] networkObjectId)
    {
        TargetSellForNetworkObjectsClientRpc(networkObjectId);
    }

    [ClientRpc]
    internal void TargetSellForNetworkObjectsClientRpc(ulong[] networkObjectId)
    {
        TargetManager.MoveNetworkObjectsToDepositDeskClient(networkObjectId);
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;

        base.OnNetworkSpawn();
    }
}