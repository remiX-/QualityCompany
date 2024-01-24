// using HarmonyLib;
// using AdvancedCompany.Network;
// using Unity.Netcode;
// using UnityEngine;
//
// namespace AdvancedCompany.Patches
// {
//     [HarmonyPatch]
//     public class NetworkObjectManager
//     {
//         private static GameObject networkPrefab;
//
//         [HarmonyPostfix]
//         [HarmonyPatch(typeof(GameNetworkManager), "Start")]
//         public static void LoadNetworkPrefab()
//         {
//             if (networkPrefab != null)
//             {
//                 return;
//             }
//
//             networkPrefab = Plugin.CustomAssets.LoadAsset<GameObject>("SellFromTerminalNetworkHandler");
//             networkPrefab.AddComponent<NetworkHandler>();
//
//             NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
//         }
//
//         [HarmonyPostfix]
//         [HarmonyPatch(typeof(StartOfRound), "Awake")]
//         public static void SpawnNetworkHandlerObject()
//         {
//             if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
//             {
//                 var networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
//                 networkHandlerHost.GetComponent<NetworkObject>().Spawn();
//             }
//         }
//     }
// }
