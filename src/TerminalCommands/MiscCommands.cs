using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Service;
using QualityCompany.Utils;
using System;
using Unity.Netcode;
using UnityEngine;

namespace QualityCompany.TerminalCommands;

internal class MiscCommands : ITerminalSubscriber
{
    private static readonly ACLogger _logger = new(nameof(MiscCommands));

    private int scrapCountToHack;

    public void Run()
    {
        if (!Plugin.Instance.PluginConfig.TerminalMiscCommandsEnabled) return;

        AdvancedTerminal.AddCommand(
            new TerminalCommandBuilder("launch")
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
    }

    private string GetTime()
    {
        return !StartOfRound.Instance.currentLevel.planetHasTime || !StartOfRound.Instance.shipDoorsEnabled ?
            "You're not on a moon. There is no time here.\n" :
            $"The time is currently {HUDManager.Instance.clockNumber.text.Replace('\n', ' ')}.\n";
    }
}