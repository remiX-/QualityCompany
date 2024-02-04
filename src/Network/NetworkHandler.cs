using QualityCompany.Components;
using QualityCompany.Service;
using QualityCompany.Utils;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace QualityCompany.Network;

public class NetworkHandler : NetworkBehaviour
{
    public static NetworkHandler Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(NetworkHandler));

    private int totalItems = 0;
    private int totalValueForSale = 0;

    [ServerRpc(RequireOwnership = false)]
    public void UpdateSellTargetServerRpc(int newTarget, string playerName)
    {
        UpdateSellTargetClientRpc(newTarget, playerName);
    }

    [ClientRpc]
    public void UpdateSellTargetClientRpc(int newTarget, string playerName)
    {
        OvertimeMonitor.targetTotalCredits = newTarget;
        OvertimeMonitor.UpdateMonitor();

        HudUtils.DisplayNotification($"Sale target has been updated to ${newTarget} by {playerName}");

        CompanyNetworkHandler.Instance.SaveData.TargetForSelling = newTarget;
        if (IsHost)
        {
            CompanyNetworkHandler.Instance.ServerSaveFileServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SellAllScrapServerRpc()
    {
        SellAllScrapClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TargetSellForNetworkObjectServerRpc(ulong networkObjectId)
    {
        TargetSellForNetworkObjectClientRpc(networkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExecuteSellAmountServerRpc()
    {
        ExecuteSellAmountClientRpc();
    }

    [ClientRpc]
    public void SellAllScrapClientRpc()
    {
        PerformSell(ScrapUtils.GetAllSellableScrapInShip());
    }

    [ClientRpc]
    public void TargetSellForNetworkObjectClientRpc(ulong networkObjectId)
    {
        MoveNetworkObjectToDepositDesk(networkObjectId);
    }

    [ClientRpc]
    public void ExecuteSellAmountClientRpc()
    {
        var depositItemsDesk = FindFirstObjectByType<DepositItemsDesk>();
        depositItemsDesk.SetTimesHeardNoiseServerRpc(5f);

        HudUtils.DisplayNotification($"Placed {totalItems} pieces of scrap onto the Company Desk for sale. Total ${totalValueForSale}!");

        totalItems = 0;
        totalValueForSale = 0;
    }

    private void PerformSell(List<GrabbableObject> scrap)
    {
        var depositItemsDesk = FindFirstObjectByType<DepositItemsDesk>();

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
            _logger.LogError($"Failed to find network object on this client: {networkObjectId}");
            return;
        }

        var depositItemsDesk = FindFirstObjectByType<DepositItemsDesk>();
        scrap.transform.parent = depositItemsDesk.deskObjectsContainer.transform;
        scrap.transform.localPosition = depositItemsDesk.transform.position + new Vector3(0f, 0f, (totalItems - 5) * 1f);
        depositItemsDesk.AddObjectToDeskServerRpc(new NetworkObjectReference(scrap.gameObject.GetComponent<NetworkObject>()));

        totalItems++;
        totalValueForSale += scrap.ActualSellValue();
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            // This is recommended in the docs, but it doesn't seem to work
            // https://lethal.wiki/dev/advanced/networking#preventing-duplication
            // Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
        }

        Instance = this;

        base.OnNetworkSpawn();
    }
}