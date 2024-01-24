using AdvancedCompany.Manager;
using UnityEngine;

namespace AdvancedCompany.TerminalCommands;

internal class MiscCommands : ITerminalSubscriber
{
    public void Run()
    {
        Logger.LogMessage("MiscCommands.StartGame");

        TerminalManager.AddCommand(
            new TerminalCommandBuilder("launch")
                .WithText("Launching!")
                .WithDescription("To launch or land the ship. Host needs to do the very first launch.")
                .EnableConfirmDeny("Are you sure you want to launch?", "Launch has been cancelled")
                .WithCondition("inTransitLandedOrLeaving", "Unable to comply. The ship is landing or taking off.", () => StartOfRound.Instance.shipDoorsEnabled && !(StartOfRound.Instance.shipHasLanded || StartOfRound.Instance.shipIsLeaving))
                .WithCondition("inTransitToMoon", "Unable to comply. The ship is already in transit to another moon.", () => !StartOfRound.Instance.shipDoorsEnabled && StartOfRound.Instance.travellingToNewLevel)
                .WithAction(() =>
                {
                    // const string alreadyTransitMessage = "Unable to comply. The ship is already in transit.";
                    // if (leverObject is null) return "!! Can't find StartGameLever !!";
                    // var lever = leverObject.GetComponent<StartMatchLever>();
                    // if (lever is null) return "!! Can't find StartMatchLever component !!";
                    //
                    // // Doors are enabled (on a moon), ship is either not landed or is leaving
                    // if (StartOfRound.Instance.shipDoorsEnabled && !(StartOfRound.Instance.shipHasLanded || StartOfRound.Instance.shipIsLeaving))
                    // {
                    //     return alreadyTransitMessage;
                    // }
                    // // Doors are disabled (in space), ship is in transit to another moon
                    // if (!StartOfRound.Instance.shipDoorsEnabled && StartOfRound.Instance.travellingToNewLevel)
                    // {
                    //     return alreadyTransitMessage;
                    // }

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

        TerminalManager.AddCommand(
            new TerminalCommandBuilder("door")
                .WithAction(() =>
                {
                    var trigger = GameObject.Find(StartOfRound.Instance.hangarDoorsClosed ? "StartButton" : "StopButton").GetComponentInChildren<InteractTrigger>();
                    trigger.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);

                    return "Toggled door.";
                })
        );

        TerminalManager.AddCommand(
            new TerminalCommandBuilder("lights")
                .WithAction(() =>
                {
                    var trigger = GameObject.Find("LightSwitch").GetComponent<InteractTrigger>();
                    trigger.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);

                    return "Toggled lights";
                })
        );

        TerminalManager.AddCommand(
            new TerminalCommandBuilder("tp")
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

        TerminalManager.AddCommand(new TerminalCommandBuilder("time").WithAction(GetTime));
    }

    private string GetTime()
    {
        return !StartOfRound.Instance.currentLevel.planetHasTime || !StartOfRound.Instance.shipDoorsEnabled ?
            "You're not on a moon. There is no time here.\n" :
            $"The time is currently {HUDManager.Instance.clockNumber.text.Replace('\n', ' ')}.\n";
    }
}