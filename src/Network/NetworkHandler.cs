using QualityCompany.Manager;
using QualityCompany.Service;
using QualityCompany.Utils;
using Unity.Netcode;
using UnityEngine.UIElements;

namespace QualityCompany.Network;

internal class NetworkHandler : NetworkBehaviour
{
    internal static NetworkHandler Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(NetworkHandler));

    [ClientRpc]
    public void SyncValuesClientRpc(int value, NetworkBehaviourReference netRef)
    {
        netRef.TryGet(out GrabbableObject prop);

        if (prop != null)
        {
            prop.transform.parent = GameUtils.ShipGameObject.transform;
            prop.scrapValue = value;
            prop.itemProperties.creditsWorth = value;
            prop.GetComponentInChildren<ScanNodeProperties>().subText = $"Value: ${value}";

            _logger.LogInfo($"Successfully synced values of {prop.itemProperties.itemName}");
        }
        else
        {
            _logger.LogInfo("Unable to resolve net ref for SyncValuesClientRpc!");
        }
    }

    // [ClientRpc]
    // public void SpawnItemClientRpc(NetworkBehaviourReference netRef)
    // {
    //     netRef.TryGet(out GameObject prop);
    //
    //     if (prop != null)
    //     {
    //         prop.transform.parent = GameUtils.ShipGameObject.transform;
    //         prop.itemProperties.creditsWorth = value;
    //         prop.GetComponentInChildren<ScanNodeProperties>().subText = $"Value: ${value}";
    //
    //         _logger.LogInfo($"Successfully synced values of {prop.itemProperties.itemName}");
    //     }
    //     else
    //     {
    //         _logger.LogInfo("Unable to resolve net ref for SyncValuesClientRpc!");
    //     }
    // }

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