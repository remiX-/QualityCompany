using AdvancedCompany.Manager.ShipTerminal;
using AdvancedCompany.Network;
using AdvancedCompany.Service;
using AdvancedCompany.Utils;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = AdvancedCompany.Utils.Logger;

namespace AdvancedCompany.TerminalCommands;

internal class MiscCommands : ITerminalSubscriber
{
    private static readonly ACLogger _logger = new(nameof(MiscCommands));

    private int scrapCountToHack;

    public void Run()
    {
        _logger.LogMessage("MiscCommands.StartGame");

        AdvancedTerminal.AddCommand(
            new TerminalCommandBuilder("launch")
                .WithText("asd")
                .WithDescription(">LAUNCH\nTo launch or land the ship. Host needs to do the very first launch.")
                // .EnableConfirmDeny("Are you sure you want to launch?", "Launch has been cancelled")
                .WithCondition("inTransitLandedOrLeaving", "Unable to comply. The ship is landing or taking off.", () => StartOfRound.Instance.shipDoorsEnabled && !(StartOfRound.Instance.shipHasLanded || StartOfRound.Instance.shipIsLeaving))
                .WithCondition("inTransitToMoon", "Unable to comply. The ship is already in transit to another moon.", () => !StartOfRound.Instance.shipDoorsEnabled && StartOfRound.Instance.travellingToNewLevel)
                .WithAction(() =>
                {
                    var leverObject = GameObject.Find("StartGameLever");
                    var lever = leverObject.GetComponent<StartMatchLever>();
                    var newState = !lever.leverHasBeenPulled;

                    lever.PullLever();
                    lever.LeverAnimation();

                    if (newState) lever.StartGame();
                    else lever.EndGame();

                    return "Initiating " + (lever.leverHasBeenPulled ? "landing" : "launch") + " sequence.";
                })
        );

        AdvancedTerminal.AddCommand(
            new TerminalCommandBuilder("door")
                .WithDescription(">DOOR\nToggle the ship door.")
                .WithAction(() =>
                {
                    var trigger = GameObject.Find(StartOfRound.Instance.hangarDoorsClosed ? "StartButton" : "StopButton").GetComponentInChildren<InteractTrigger>();
                    trigger.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);

                    return "Toggled door.";
                })
        );

        AdvancedTerminal.AddCommand(
            new TerminalCommandBuilder("lights")
                .WithDescription(">LIGHTS\nToggle the lights.")
                .WithAction(() =>
                {
                    var trigger = GameObject.Find("LightSwitch").GetComponent<InteractTrigger>();
                    trigger.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);

                    return "Toggled lights";
                })
        );

        AdvancedTerminal.AddCommand(
            new TerminalCommandBuilder("tp")
                .WithDescription(">TP\nTeleport the currently active player on the view monitor to the ship. Must have a teleporter.")
                .WithAction(() =>
                {
                    var teleporterObject = GameObject.Find("Teleporter(Clone)");
                    if (teleporterObject is null) return "ERROR: You don't have a teleporter!";

                    var teleporter = teleporterObject.GetComponent<ShipTeleporter>();
                    if (teleporter is null) return "ERROR: Can't find ShipTeleporter component";

                    if (!teleporter.buttonTrigger.interactable) return "Teleporter is on cooldown!";

                    teleporter.buttonTrigger.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);
                    return "Teleporting active viewing player";
                })
        );

        AdvancedTerminal.AddCommand(new TerminalCommandBuilder("time")
            .WithDescription(">TIME\nGet the current time whilst on a moon.")
            .WithAction(GetTime));

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
                        Logger.LogMessage($"hacky: {input}");
                        scrapCountToHack = Convert.ToInt32(input);

                        if (scrapCountToHack <= 0) return false;

                        Logger.LogMessage("ACTION");
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
                            Logger.LogMessage($"Spawned in {scrap.name} for {scrapValue}");

                            RoundManager.Instance.scrapCollectedThisRound.Add(scrap.GetComponent<GrabbableObject>());
                            // scrap.transform.parent = GameUtils.ShipGameObject.transform;
                        }

                        return true;
                    })
                    .WithAction(() =>
                    {
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