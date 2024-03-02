using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Network;
using QualityCompany.Service;
using System;
using Unity.Netcode;
using UnityEngine;
using static QualityCompany.Service.ServiceRegistry;

namespace QualityCompany.TerminalCommands;

internal class Commands4Debug
{
    private static readonly ModLogger Logger = new(nameof(Commands4Debug));

    private static int scrapCountToHack;

    [TerminalCommand]
    private static TerminalCommandBuilder Run()
    {
        if (!Plugin.Instance.PluginConfig.TerminalDebugCommandsEnabled) return null;

        return new TerminalCommandBuilder("hack")
            .WithHelpDescription("Spawn some ez lewt.")
            .WithCommandDescription("Please enter a number of scrap items to spawn.\neg: hack 5")
            .WithCondition("isHost", "You are not host.", () => NetworkManager.Singleton.IsHost)
            .AddTextReplacement("[scrapCountToHack]", () => scrapCountToHack.ToString())
            .WithSubCommand(new TerminalSubCommandBuilder("<count>")
                .WithDescription("Hack in <count> number of items.")
                .WithMessage("Hacked in [scrapCountToHack] items")
                .WithConditions("isHost")
                .WithInputMatch(@"(\d+$)$")
                .WithPreAction(input =>
                {
                    Logger.TryLogDebug($"Hack: IsHost? {NetworkManager.Singleton.IsHost}");
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

                    Logger.TryLogDebug($"Saved item data to {Application.persistentDataPath}");
                    File.WriteAllText(Path.Combine(Application.persistentDataPath, "game_items.json"), JsonConvert.SerializeObject(dict));
#endif

                    HackInScrap();

                    return true;
                })
                .WithAction(() =>
                {
                    Logger.TryLogDebug("Hack: WithAction?");
                })
            );
    }

    private static void HackInScrap()
    {
        var itemsList = StartOfRound.Instance.allItemsList.itemsList;
        var currentPlayerLocation = GameNetworkManager.Instance.localPlayerController.transform.position;
        for (var i = 0; i < scrapCountToHack; i++)
        {

            var item = itemsList[Randomizer.GetInt(0, itemsList.Count)];
            while (!item.isScrap)
            {
                item = itemsList[Randomizer.GetInt(0, itemsList.Count)];
            }

            var itemToSpawn = item.spawnPrefab;

            var scrap = UnityEngine.Object.Instantiate(itemToSpawn, currentPlayerLocation, Quaternion.identity);
            var itemGrabObj = scrap.GetComponent<GrabbableObject>();

            if (itemGrabObj is null)
            {
                Logger.TryLogDebug($"{itemToSpawn.name}: did not have a GrabbableObject component");
                continue;
            }

            var scrapValue = GetScrapValue(itemGrabObj.itemProperties);
            Logger.TryLogDebug($"HACK: #{i + 1} - {itemToSpawn.name} for {scrapValue}");
            scrap.GetComponent<NetworkObject>().Spawn();

            NetworkHandler.Instance.SyncValuesClientRpc(scrapValue, new NetworkBehaviourReference(itemGrabObj));
        }
    }

    private static int GetScrapValue(Item itemProps)
    {
        var min = itemProps.minValue;
        var max = itemProps.maxValue;
        if (min == 0 || max == 0)
        {
            min = 1;
            max = 100;
        }
        else if (min > max)
        {
            min = itemProps.maxValue;
            max = itemProps.minValue;
        }
        return Randomizer.GetInt(min, max) / 2;
    }
}