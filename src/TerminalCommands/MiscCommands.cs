using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Service;
using QualityCompany.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QualityCompany.TerminalCommands;

internal class MiscCommands
{
    private static readonly ModLogger Logger = new(nameof(MiscCommands));

    [TerminalCommand]
    private static TerminalCommandBuilder LaunchCommand()
    {
        if (!Plugin.Instance.PluginConfig.TerminalMiscCommandsEnabled) return null;

        return new TerminalCommandBuilder("launch")
            .WithHelpDescription("To launch or land the ship. Host needs to do the very first launch.")
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
            .WithHelpDescription("Toggle the ship door.")
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
            .WithHelpDescription("Teleport the currently active player on the view monitor to the ship. Must have a teleporter.")
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
            .WithHelpDescription("Toggle the lights.")
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
            .WithHelpDescription("Get the current time whilst on a moon.")
            .WithAction(GetTime);

        static string GetTime()
        {
            return !StartOfRound.Instance.currentLevel.planetHasTime || !StartOfRound.Instance.shipDoorsEnabled ?
                "You're not on a moon. There is no time here.\n" :
                $"The time is currently {HUDManager.Instance.clockNumber.text.Replace('\n', ' ')}.\n";
        }
    }

    [TerminalCommand]
    private static TerminalCommandBuilder QuickSwitchCommand()
    {
        if (!Plugin.Instance.PluginConfig.ExperimentalFeaturesEnabled) return null;

        return new TerminalCommandBuilder("vs")
            .WithHelpDescription("Execute 'switch' but easier.")
            .WithAction(() =>
            {
                var switchObject = GameObject.Find("CameraMonitorSwitchButton");
                if (switchObject is null) return "ERROR: Failed to find CameraMonitorSwitchButton :/";

                var trigger = switchObject.transform.GetChild(0).GetComponent<InteractTrigger>();
                if (trigger is null) return "ERROR: Failed to find CameraMonitorSwitchButton InteractTrigger";

                if (!trigger.interactable) return "Teleporter is on cooldown!";

                trigger.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);
                return $"Switched to {GameUtils.StartOfRound.mapScreenPlayerName}!";
            });
    }

    private static string? _playerSwitchName;
    private static int _playerSwitchIndex = -1;
    [TerminalCommand]
    private static TerminalCommandBuilder Command()
    {
        if (!Plugin.Instance.PluginConfig.ExperimentalFeaturesEnabled) return null;

        return new TerminalCommandBuilder("vw")
            .WithHelpDescription("Execute 'switch' to a player but easier.")
            .WithSubCommand(new TerminalSubCommandBuilder("<player>")
                .WithMessage("Switched to [playerSwitchName]")
                .WithConditions("validPlayer")
                .WithInputMatch(@"^(\w+)$")
                .WithPreAction(input =>
                {
                    // Reset some vars
                    input = input.ToLower();
                    _playerSwitchName = null;
                    _playerSwitchIndex = -1;

                    var playerNames = new List<string>();
                    for (var i = 0; i < GameUtils.StartOfRound.mapScreen.radarTargets.Count; i++)
                    {
                        var playerName = GameUtils.StartOfRound.mapScreen.radarTargets[i].name;
                        Logger.TryLogDebug($"view cmd: player {i}: {playerName}");
                        playerNames.Add(playerName.ToLower());
                    }

                    for (var i = 0; i < playerNames.Count; i++)
                    {
                        if (playerNames[i] != input && !playerNames[i].StartsWith(input)) continue;

                        _playerSwitchName = playerNames[i];
                        _playerSwitchIndex = i;
                        break;
                    }

                    _playerSwitchName ??= input;

                    return true;
                })
                .WithAction(() =>
                {
                    Logger.TryLogDebug($"view command: {_playerSwitchName} @ {_playerSwitchIndex}");
                    GameUtils.StartOfRound.mapScreen.SwitchRadarTargetAndSync(_playerSwitchIndex);
                })
            )
            .AddTextReplacement("[playerSwitchName]", () => _playerSwitchName ?? "unknown")
            .WithCondition("validPlayer", "Invalid player: [playerSwitchName]", () => _playerSwitchIndex >= 0);
    }
}