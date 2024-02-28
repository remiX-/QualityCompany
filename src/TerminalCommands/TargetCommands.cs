using QualityCompany.Manager;
using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Modules.Ship;
using QualityCompany.Service;
using QualityCompany.Utils;

namespace QualityCompany.TerminalCommands;

internal class TargetCommands
{
    private static readonly ModLogger Logger = new(nameof(TargetCommands));

    [TerminalCommand]
    private static TerminalCommandBuilder CreateTargetCommand()
    {
        if (!Plugin.Instance.PluginConfig.TerminalTargetCommandsEnabled) return null;

        return new TerminalCommandBuilder("target")
            .WithHelpDescription("Set a target sell requirement for the target monitor.")
            .WithCommandDescription("Please enter an amount.\neg: target 2000\n\n[targetExplanation]")
            .WithSubCommand(new TerminalSubCommandBuilder("<amount>")
                .WithDescription("The desired target wanted upon leaving The Company Building.")
                .WithMessage("[companyBuyingRateWarning]Target has been set to [targetSetTo].\nMonitor Values:\n[targetMonitorValues]\n\n[targetExplanation]")
                // .WithConditions("landedAtCompany")
                .WithInputMatch(@"^(\d+$)$")
                .WithPreAction(input =>
                {
                    if (!int.TryParse(input, out var amount)) return false;

                    // TODO: this UpdateTarget action should be part of `WithAction`! But seemingly there is a bug in the Terminal API
                    // if (!GameUtils.IsLandedOnCompany()) return false;

                    TargetManager.UpdateTarget(amount, GameNetworkManager.Instance.localPlayerController.playerUsername);

                    return true;
                })
                .WithAction(() => Logger.TryLogDebug("EXEC target.Action???"))
            )
            .WithCondition("landedAtCompany", "ERROR: Usage of this feature is only permitted within Company bounds\n\nPlease land at 71-Gordion and repeat command.", GameUtils.IsLandedOnCompany)
            .AddTextReplacement("[targetMonitorValues]", InfoMonitor.GetText)
            .AddTextReplacement("[targetExplanation]", GetTargetExplanation)
            .AddTextReplacement("[targetSetTo]", () => InfoMonitor.Instance.TargetTotalCredits.ToString());
    }

    private static string GetTargetExplanation()
    {
        return
            $@"You want a target of [targetSetTo], so how do we do this? The target, in a nutshell, is how many credits you want AFTER leaving the company.
This includes overtime bonus for selling over and beyond the current Profit Quota. So, the InfoMonitor tries to take all of this into consideration:
* Group credits = ${GameUtils.Terminal.groupCredits}
* Profit Quota = ${GameUtils.TimeOfDay.profitQuota}
* Amount fulfilled (sold) = ${GameUtils.TimeOfDay.quotaFulfilled}
* Overtime bonus (on leaving) = ${InfoMonitor.Instance.CalculatedOvertime}

This is done in order to calculate a correct ""NEEDED"" amount as close as possible. This is sitting at ${InfoMonitor.Instance.CalculatedNeededToReachTarget}.
If this is at $0 then you do not need to sell more, unless you now choose to set a NEW target amount.

This does not (yet) account for:
* Value of scrap on the desk = ${InfoMonitor.Instance.CalculatedDeskTotal}
* Fees of deaths whilst at The Company

Hope this helps!";
    }
}

