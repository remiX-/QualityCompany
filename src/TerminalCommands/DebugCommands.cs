using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Service;
using QualityCompany.Utils;
using System;
using Unity.Netcode;
using UnityEngine;

namespace QualityCompany.TerminalCommands;

internal class DebugCommands : ITerminalSubscriber
{
    private static readonly ACLogger _logger = new(nameof(DebugCommands));

    private int scrapCountToHack;

    public void Run()
    {
        if (!Plugin.Instance.PluginConfig.TerminalDebugCommandsEnabled) return;

        AdvancedTerminal.AddCommand(
            new TerminalCommandBuilder("hack")
                .WithDescription(">hack <count>\nSpawn some ez lewt.")
                .WithText("Please enter a number of scrap items to spawn.\neg: hack 5")
                .WithCondition("isHost", "You are not host.", () => NetworkManager.Singleton.IsHost)
                .AddTextReplacement("[scrapCountToHack]", () => scrapCountToHack.ToString())
                .WithSubCommand(new TerminalSubCommandBuilder("<ha>")
                    .WithMessage("Hacked in [scrapCountToHack] items")
                    .WithConditions("isHost")
                    .WithInputMatch(@"(\d+$)$")
                    .WithPreAction(input =>
                    {
                        scrapCountToHack = Convert.ToInt32(input);

                        if (scrapCountToHack <= 0) return false;

                        for (var i = 0; i < scrapCountToHack; i++)
                        {
                            var rand = new System.Random();
                            var nextScrap = rand.Next(16, 68);
                            var scrap = UnityEngine.Object.Instantiate(StartOfRound.Instance.allItemsList.itemsList[nextScrap].spawnPrefab, GameNetworkManager.Instance.localPlayerController.transform.position, Quaternion.identity);
                            scrap.GetComponent<GrabbableObject>().fallTime = 0f;
                            var scrapValue = rand.Next(40, 120);
                            scrap.AddComponent<ScanNodeProperties>().scrapValue = scrapValue;
                            scrap.GetComponent<GrabbableObject>().scrapValue = scrapValue;
                            scrap.GetComponent<NetworkObject>().Spawn();
                            _logger.LogDebug($"Spawned in {scrap.name} for {scrapValue}");

                            RoundManager.Instance.scrapCollectedThisRound.Add(scrap.GetComponent<GrabbableObject>());
                            scrap.transform.parent = GameUtils.ShipGameObject.transform;
                        }

                        return true;
                    })
                )
        );
    }

    private string GetTime()
    {
        return !StartOfRound.Instance.currentLevel.planetHasTime || !StartOfRound.Instance.shipDoorsEnabled ?
            "You're not on a moon. There is no time here.\n" :
            $"The time is currently {HUDManager.Instance.clockNumber.text.Replace('\n', ' ')}.\n";
    }
}