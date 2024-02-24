using Newtonsoft.Json;
using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Network;
using QualityCompany.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace QualityCompany.TerminalCommands;

internal class DebugCommands
{
    private static readonly ModLogger _logger = new(nameof(DebugCommands));

    private static int scrapCountToHack;

    [TerminalCommand]
    private static TerminalCommandBuilder Run()
    {
        if (!Plugin.Instance.PluginConfig.TerminalDebugCommandsEnabled) return null;

        return new TerminalCommandBuilder("hack")
            .WithDescription("> hack <count>\nSpawn some ez lewt.")
            .WithText("Please enter a number of scrap items to spawn.\neg: hack 5")
            .WithCondition("isHost", "You are not host.", () => NetworkManager.Singleton.IsHost)
            .AddTextReplacement("[scrapCountToHack]", () => scrapCountToHack.ToString())
            .WithSubCommand(new TerminalSubCommandBuilder("<count>")
                .WithDescription("Hack in <count> number of items.")
                .WithMessage("Hacked in [scrapCountToHack] items")
                .WithConditions("isHost")
                .WithInputMatch(@"(\d+$)$")
                .WithPreAction(input =>
                {
                    _logger.LogDebug($"Hack: IsHost? {NetworkManager.Singleton.IsHost}");
                    // TODO: bug here, this shouldn't be in "PreAction"
                    if (!NetworkManager.Singleton.IsHost) return false;

                    scrapCountToHack = Convert.ToInt32(input);

                    if (scrapCountToHack <= 0) return false;
                    scrapCountToHack = Math.Min(100, scrapCountToHack);

#if DEBUG
                    var dict = StartOfRound.Instance.allItemsList.itemsList.ToDictionary<Item, string, dynamic>(item => item.name, item => new
                    {
                        item.name,
                        item.minValue,
                        item.maxValue,
                        item.batteryUsage,
                        item.isScrap,
                        item.twoHanded,
                        item.weight,
                        item.creditsWorth,
                        item.itemSpawnsOnGround,
                        item.itemId
                    });

                    _logger.LogDebug($"Saved item data to {Path.Combine(Plugin.Instance.PluginPath)}");
                    File.WriteAllText(Path.Combine(Plugin.Instance.PluginPath, "game_items.json"), JsonConvert.SerializeObject(dict));
#endif

                    var currentPlayerLocation = GameNetworkManager.Instance.localPlayerController.transform.position;
                    for (var i = 0; i < scrapCountToHack; i++)
                    {
                        _logger.LogDebug($"Hacking in item #{i}");

                        var rand = new System.Random();
                        var nextScrap = rand.Next(16, 68);
                        var itemToSpawn = StartOfRound.Instance.allItemsList.itemsList[nextScrap].spawnPrefab;

                        var scrap = UnityEngine.Object.Instantiate(itemToSpawn, currentPlayerLocation, Quaternion.identity);
                        var itemGrabObj = scrap.GetComponent<GrabbableObject>();

                        if (itemGrabObj is null)
                        {
                            _logger.LogDebug($"{itemToSpawn.name}: did not have a GrabbableObject component");
                            continue;
                        }

                        var scrapValue = rand.Next(itemGrabObj.itemProperties.minValue, itemGrabObj.itemProperties.maxValue) / 2;
                        _logger.LogDebug($" > spawned in {itemToSpawn.name} for {scrapValue}");
                        scrap.GetComponent<NetworkObject>().Spawn();

                        // RoundManager.Instance.scrapCollectedThisRound.Add(scrap.GetComponent<GrabbableObject>());
                        NetworkHandler.Instance.SyncValuesClientRpc(scrapValue, new NetworkBehaviourReference(itemGrabObj));

                        _logger.LogDebug(" > done");
                    }

                    return true;
                })
                .WithAction(() =>
                {
                    _logger.LogDebug("Hack: WithAction?");
                })
            );
    }

    private string GetTime()
    {
        return !StartOfRound.Instance.currentLevel.planetHasTime || !StartOfRound.Instance.shipDoorsEnabled ?
            "You're not on a moon. There is no time here.\n" :
            $"The time is currently {HUDManager.Instance.clockNumber.text.Replace('\n', ' ')}.\n";
    }
}