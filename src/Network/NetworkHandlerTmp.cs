// using System.Collections.Generic;
// using System.Linq;
// using AdvancedCompany.Game;
// using AdvancedCompany.Scrap;
// using Unity.Netcode;
// using UnityEngine;
//
// namespace AdvancedCompany.Network;
//
// public class NetworkHandler : NetworkBehaviour
// {
//     public static NetworkHandler Instance { get; private set; }
//
//     private static int totalItems = 0;
//     private static int totalValueForSale = 0;
//
//     // public static void SellAllScrap()
//     // {
//     //     Logger.LogDebug("NetworkManager.SellAllScrap");
//     //     PerformSell(ScrapHelpers.GetAllScrapInShip());
//     // }
//     //
//     // public static void SellAmount(List<GrabbableObject> itemsToSell)
//     // {
//     //     Logger.LogDebug("NetworkManager.SellAmount");
//     //     PerformSell(itemsToSell);
//     // }
//
//     [ServerRpc(RequireOwnership = false)]
//     public static void SellAllScrapServerRpc()
//     {
//         Logger.LogDebug("NetworkManager.SellAllScrapServerRpc");
//         SellAllScrapClientRpc();
//     }
//
//     [ServerRpc(RequireOwnership = false)]
//     public static void TargetSellForNetworkObjectServerRpc(ulong networkObjectId)
//     {
//         Logger.LogDebug($"NetworkManager.TargetSellForNetworkObjectServerRpc => networkObjectId: {networkObjectId}");
//         TargetSellForNetworkObjectClientRpc(networkObjectId);
//     }
//
//     [ServerRpc(RequireOwnership = false)]
//     public static void ExecuteSellAmountServerRpc()
//     {
//         Logger.LogDebug("NetworkManager.ExecuteSellAmountServerRpc");
//         ExecuteSellAmountClientRpc();
//     }
//
//     [ClientRpc]
//     public static void SellAllScrapClientRpc()
//     {
//         Logger.LogDebug("NetworkManager.SellAllScrapClientRpc");
//         PerformSell(ScrapHelpers.GetAllScrapInShip());
//     }
//
//     [ClientRpc]
//     public static void TargetSellForNetworkObjectClientRpc(ulong networkObjectId)
//     {
//         Logger.LogDebug($"NetworkManager.TargetSellForNetworkObjectClientRpc => networkObjectId: {networkObjectId}");
//         MoveNetworkObjectToDepositDesk(networkObjectId);
//     }
//
//     [ClientRpc]
//     public static void ExecuteSellAmountClientRpc()
//     {
//         Logger.LogDebug("NetworkManager.ExecuteSellAmountClientRpc");
//
//         Notification.Display($"Placed {totalItems} pieces of scrap onto the Company Desk for sale. Total ${totalValueForSale}!");
//
//         totalItems = 0;
//         totalValueForSale = 0;
//     }
//
//     private static void PerformSell(List<GrabbableObject> scrap)
//     {
//         var depositItemsDesk = FindObjectOfType<DepositItemsDesk>();
//
//         var index = 0;
//         scrap.ForEach(item =>
//         {
//             item.transform.parent = depositItemsDesk.deskObjectsContainer.transform;
//             item.transform.localPosition = depositItemsDesk.transform.position + new Vector3(0f, 0f, (index - 5) * 1f);
//             depositItemsDesk.AddObjectToDeskServerRpc(new NetworkObjectReference(item.gameObject.GetComponent<NetworkObject>()));
//
//             index++;
//         });
//
//         var totalValue = ScrapHelpers.SumScrapListSellValue(scrap);
//         Notification.Display($"Placed {scrap.Count} pieces of scrap onto the Company Desk for sale. Total ${totalValue}!");
//     }
//
//     private static void MoveNetworkObjectToDepositDesk(ulong networkObjectId)
//     {
//         var scrap = ScrapHelpers.GetAllScrapInShip().FirstOrDefault(x => x.NetworkObjectId == networkObjectId);
//         if (scrap is null)
//         {
//             Logger.LogDebug($"Failed to find network object on this client: {networkObjectId}");
//             return;
//         }
//
//         var depositItemsDesk = FindObjectOfType<DepositItemsDesk>();
//         scrap.transform.parent = depositItemsDesk.deskObjectsContainer.transform;
//         scrap.transform.localPosition = depositItemsDesk.transform.position + new Vector3(0f, 0f, (totalItems - 5) * 1f);
//         depositItemsDesk.AddObjectToDeskServerRpc(new NetworkObjectReference(scrap.gameObject.GetComponent<NetworkObject>()));
//
//         totalItems++;
//         totalValueForSale += scrap.ActualSellValue();
//     }
//
//     public override void OnNetworkSpawn()
//     {
//         Logger.LogDebug("NetworkManager.OnNetworkSpawn");
//         if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
//         {
//             Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
//         }
//
//         Instance = this;
//
//         base.OnNetworkSpawn();
//     }
// }