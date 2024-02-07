using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Service;
using UnityEngine;

namespace QualityCompany.TerminalCommands;

internal class MiscCommands
{
    private static readonly ACLogger _logger = new(nameof(MiscCommands));

    [TerminalCommand]
    private static TerminalCommandBuilder LaunchCommand()
    {
        if (!Plugin.Instance.PluginConfig.TerminalMiscCommandsEnabled) return null;

        return new TerminalCommandBuilder("launch")
            .WithDescription(">LAUNCH\nTo launch or land the ship. Host needs to do the very first launch.")
            .WithCondition("inTransitLandedOrLeaving", "Unable to comply. The ship is landing or taking off.",
                () => StartOfRound.Instance.shipDoorsEnabled &&
                      !(StartOfRound.Instance.shipHasLanded || StartOfRound.Instance.shipIsLeaving))
            .WithCondition("inTransitToMoon", "Unable to comply. The ship is already in transit to another moon.",
                () => !StartOfRound.Instance.shipDoorsEnabled && StartOfRound.Instance.travellingToNewLevel)
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
            });
    }

    [TerminalCommand]
    private static TerminalCommandBuilder DoorCommand()
    {
        if (!Plugin.Instance.PluginConfig.TerminalMiscCommandsEnabled) return null;

        return new TerminalCommandBuilder("door")
            .WithDescription(">DOOR\nToggle the ship door.")
            .WithAction(() =>
            {
                var trigger = GameObject.Find(StartOfRound.Instance.hangarDoorsClosed ? "StartButton" : "StopButton")
                    .GetComponentInChildren<InteractTrigger>();
                trigger.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);

                return "Toggled door.";
            });
    }

    [TerminalCommand]
    private static TerminalCommandBuilder TeleportCommand()
    {
        if (!Plugin.Instance.PluginConfig.TerminalMiscCommandsEnabled) return null;

        return new TerminalCommandBuilder("tp")
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
            });
    }

    [TerminalCommand]
    private static TerminalCommandBuilder LightsCommand()
    {
        if (!Plugin.Instance.PluginConfig.TerminalMiscCommandsEnabled) return null;

        return new TerminalCommandBuilder("lights")
            .WithDescription(">LIGHTS\nToggle the lights.")
            .WithAction(() =>
            {
                var trigger = GameObject.Find("LightSwitch").GetComponent<InteractTrigger>();
                trigger.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);

                return "Toggled lights";
            });
    }

    [TerminalCommand]
    private static TerminalCommandBuilder TimeCommand()
    {
        if (!Plugin.Instance.PluginConfig.TerminalMiscCommandsEnabled) return null;

        return new TerminalCommandBuilder("time")
            .WithDescription(">TIME\nGet the current time whilst on a moon.")
            .WithAction(GetTime);
    }

    private static string GetTime()
    {
        return !StartOfRound.Instance.currentLevel.planetHasTime || !StartOfRound.Instance.shipDoorsEnabled ?
            "You're not on a moon. There is no time here.\n" :
            $"The time is currently {HUDManager.Instance.clockNumber.text.Replace('\n', ' ')}.\n";
    }
}