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
    private static readonly ModLogger Logger = new(nameof(TargetManager));

    internal static void UpdateTarget(int targetAmount, string updatedBy)
    {
        Logger.LogDebug($"UpdateTarget => {Plugin.Instance.PluginConfig.NetworkingEnabled}");
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
        MoveNetworkObjectsToDepositDeskClient(
            ScrapUtils
                .GetAllSellableScrapInShip()
                .Select(x => x.NetworkObjectId)
                .ToArray()
        );
    }

    internal static void SellAllTargetedScrap(List<GrabbableObject> scrap)
    {
        var scrapNetworkObjectIds = scrap
            .Select(x => x.NetworkObjectId)
            .ToArray();

        if (Plugin.Instance.PluginConfig.NetworkingEnabled)
        {
            NetworkHandler.Instance.TargetSellForNetworkObjectsServerRpc(scrapNetworkObjectIds);
        }
        else
        {
            MoveNetworkObjectsToDepositDeskClient(scrapNetworkObjectIds);
        }
    }

    internal static void MoveNetworkObjectsToDepositDeskClient(ulong[] networkObjectId)
    {
        var scrapToSell = ScrapUtils
            .GetAllScrapInShip()
            .Where(x =>
            {
                var contains = networkObjectId.Contains(x.NetworkObjectId);
                if (!contains) Logger.LogError($"Failed to find network object on this client: {networkObjectId}");
                return contains;
            })
            .ToList();

        if (scrapToSell.Count == 0) return;

        var depositItemsDesk = Object.FindFirstObjectByType<DepositItemsDesk>();

        var totalItems = scrapToSell.Count;
        var totalValue = 0;
        for (var index = 0; index < totalItems; index++)
        {
            var scrap = scrapToSell[index];
            scrap.transform.parent = depositItemsDesk.deskObjectsContainer.transform;
            scrap.transform.localPosition = depositItemsDesk.transform.position + new Vector3(0f, 0f, (index - 5) * 1f);
            depositItemsDesk.AddObjectToDeskServerRpc(scrap.gameObject.GetComponent<NetworkObject>());

            totalValue += scrap.ActualSellValue();
        }

        depositItemsDesk.SetTimesHeardNoiseServerRpc(5f);

        HudUtils.DisplayNotification($"Placed {totalItems} pieces of scrap onto the Company Desk for sale. Total ${totalValue}!");

        InfoMonitor.UpdateMonitor();
    }
    #endregion
}
