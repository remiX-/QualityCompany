using QualityCompany.Manager.Saves;
using QualityCompany.Modules.Ship;
using QualityCompany.Network;
using QualityCompany.Service;
using QualityCompany.Utils;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace QualityCompany.Manager;

internal class TargetManager
{
    private static readonly ACLogger Logger = new(nameof(TargetManager));

    private static int _totalItems;
    private static int _totalValueForSale;

    internal static void UpdateTarget(int targetAmount, string updatedBy)
    {
        if (Plugin.Instance.PluginConfig.NetworkingEnabled)
        {
            NetworkHandler.Instance.UpdateSellTargetServerRpc(targetAmount, GameNetworkManager.Instance.localPlayerController.playerUsername);
        }
        else
        {
            UpdateTargetClient(targetAmount, updatedBy);
        }
    }

    internal static void UpdateTargetClient(int targetAmount, string updatedBy)
    {
        HudUtils.DisplayNotification($"Sale target has been updated to ${targetAmount} by {updatedBy}");

        SaveManager.SaveData.TargetForSelling = targetAmount;
        SaveManager.Save();

        InfoMonitor.UpdateMonitor();
    }

    #region Selling
    internal static void SellAllScrap()
    {
        if (Plugin.Instance.PluginConfig.NetworkingEnabled)
        {
            NetworkHandler.Instance.SellAllScrapServerRpc();
        }
        else
        {
            SellAllScrapClient();
        }
    }

    internal static void SellAllScrapClient()
    {
        PerformSell(ScrapUtils.GetAllSellableScrapInShip());
    }

    private static void PerformSell(List<GrabbableObject> scrap)
    {
        var depositItemsDesk = Object.FindFirstObjectByType<DepositItemsDesk>();

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

    internal static void SellAllTargetedScrap(List<GrabbableObject> scrap)
    {
        if (Plugin.Instance.PluginConfig.NetworkingEnabled)
        {
            foreach (var scrapNetworkObjectId in scrap.Select(x => x.NetworkObjectId))
            {
                NetworkHandler.Instance.TargetSellForNetworkObjectServerRpc(scrapNetworkObjectId);
            }

            NetworkHandler.Instance.ExecuteSellAmountServerRpc();
        }
        else
        {
            foreach (var scrapNetworkObjectId in scrap.Select(x => x.NetworkObjectId))
            {
                MoveNetworkObjectToDepositDeskClient(scrapNetworkObjectId);
            }

            ExecuteTargetedSellOrderClient();
        }
    }

    internal static void MoveNetworkObjectToDepositDeskClient(ulong networkObjectId)
    {
        var scrap = ScrapUtils.GetAllSellableScrapInShip().FirstOrDefault(x => x.NetworkObjectId == networkObjectId);
        if (scrap is null)
        {
            Logger.LogError($"Failed to find network object on this client: {networkObjectId}");
            return;
        }

        var depositItemsDesk = Object.FindFirstObjectByType<DepositItemsDesk>();
        scrap.transform.parent = depositItemsDesk.deskObjectsContainer.transform;
        scrap.transform.localPosition = depositItemsDesk.transform.position + new Vector3(0f, 0f, (_totalItems - 5) * 1f);
        depositItemsDesk.AddObjectToDeskServerRpc(new NetworkObjectReference(scrap.gameObject.GetComponent<NetworkObject>()));

        _totalItems++;
        _totalValueForSale += scrap.ActualSellValue();
    }

    internal static void ExecuteTargetedSellOrderClient()
    {
        var depositItemsDesk = Object.FindFirstObjectByType<DepositItemsDesk>();
        depositItemsDesk.SetTimesHeardNoiseServerRpc(5f);

        HudUtils.DisplayNotification($"Placed {_totalItems} pieces of scrap onto the Company Desk for sale. Total ${_totalValueForSale}!");

        _totalItems = 0;
        _totalValueForSale = 0;

        InfoMonitor.UpdateMonitor();
    }

    #endregion
}
