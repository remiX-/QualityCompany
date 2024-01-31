using AdvancedCompany.Components;
using AdvancedCompany.Service;
using AdvancedCompany.Utils;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace AdvancedCompany.Network;

public class NetworkHandler : NetworkBehaviour
{
    public static NetworkHandler Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(NetworkHandler));

    private int totalItems = 0;
    private int totalValueForSale = 0;

    [ServerRpc(RequireOwnership = false)]
    public void UpdateSellTargetServerRpc(int newTarget, string playerName)
    {
        _logger.LogDebug($"NetworkManager.UpdateSellTargetServerRpc -> {newTarget}, {playerName}");
        UpdateSellTargetClientRpc(newTarget, playerName);
    }

    [ClientRpc]
    public void UpdateSellTargetClientRpc(int newTarget, string playerName)
    {
        _logger.LogDebug($"NetworkManager.UpdateSellTargetClientRpc -> {newTarget}, {playerName}");
        OvertimeMonitor.targetTotalCredits = newTarget;
        OvertimeMonitor.UpdateMonitor();

        HudUtils.DisplayNotification($"Sale target has been updated to ${newTarget} by {playerName}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SellAllScrapServerRpc()
    {
        _logger.LogDebug("NetworkManager.SellAllScrapServerRpc");
        SellAllScrapClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TargetSellForNetworkObjectServerRpc(ulong networkObjectId)
    {
        _logger.LogDebug($"NetworkManager.TargetSellForNetworkObjectServerRpc => networkObjectId: {networkObjectId}");
        TargetSellForNetworkObjectClientRpc(networkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExecuteSellAmountServerRpc()
    {
        _logger.LogDebug("NetworkManager.ExecuteSellAmountServerRpc");
        ExecuteSellAmountClientRpc();
    }

    [ClientRpc]
    public void SellAllScrapClientRpc()
    {
        _logger.LogDebug("NetworkManager.SellAllScrapClientRpc");
        PerformSell(ScrapUtils.GetAllSellableScrapInShip());
    }

    [ClientRpc]
    public void TargetSellForNetworkObjectClientRpc(ulong networkObjectId)
    {
        _logger.LogDebug($"NetworkManager.TargetSellForNetworkObjectClientRpc => networkObjectId: {networkObjectId}");
        MoveNetworkObjectToDepositDesk(networkObjectId);
    }

    [ClientRpc]
    public void ExecuteSellAmountClientRpc()
    {
        _logger.LogDebug("NetworkManager.ExecuteSellAmountClientRpc");

        var depositItemsDesk = FindObjectOfType<DepositItemsDesk>();
        depositItemsDesk.SetTimesHeardNoiseServerRpc(5f);

        HudUtils.DisplayNotification($"Placed {totalItems} pieces of scrap onto the Company Desk for sale. Total ${totalValueForSale}!");

        totalItems = 0;
        totalValueForSale = 0;
    }

    private void PerformSell(List<GrabbableObject> scrap)
    {
        var depositItemsDesk = FindObjectOfType<DepositItemsDesk>();

        var index = 0;
        scrap.ForEach(item =>
        {
            item.transform.parent = depositItemsDesk.deskObjectsContainer.transform;
            item.transform.localPosition = depositItemsDesk.transform.position + new Vector3(0f, 0f, (index - 5) * 1f);
            depositItemsDesk.AddObjectToDeskServerRpc(new NetworkObjectReference(item.gameObject.GetComponent<NetworkObject>()));

            index++;
        });

        depositItemsDesk.SetTimesHeardNoiseServerRpc(5f);

        var totalValue = ScrapUtils.SumScrapListSellValue(scrap);
        HudUtils.DisplayNotification($"Placed {scrap.Count} pieces of scrap onto the Company Desk for sale. Total ${totalValue}!");
    }

    private void MoveNetworkObjectToDepositDesk(ulong networkObjectId)
    {
        var scrap = ScrapUtils.GetAllSellableScrapInShip().FirstOrDefault(x => x.NetworkObjectId == networkObjectId);
        if (scrap is null)
        {
            _logger.LogDebug($"Failed to find network object on this client: {networkObjectId}");
            return;
        }

        var depositItemsDesk = FindObjectOfType<DepositItemsDesk>();
        scrap.transform.parent = depositItemsDesk.deskObjectsContainer.transform;
        scrap.transform.localPosition = depositItemsDesk.transform.position + new Vector3(0f, 0f, (totalItems - 5) * 1f);
        depositItemsDesk.AddObjectToDeskServerRpc(new NetworkObjectReference(scrap.gameObject.GetComponent<NetworkObject>()));

        totalItems++;
        totalValueForSale += scrap.ActualSellValue();
    }

    public override void OnNetworkSpawn()
    {
        _logger.LogDebug("NetworkManager.OnNetworkSpawn");
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
        }

        Instance = this;

        base.OnNetworkSpawn();
    }
}