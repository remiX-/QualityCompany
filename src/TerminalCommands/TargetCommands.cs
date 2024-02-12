using QualityCompany.Manager;
using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Modules.Ship;
using QualityCompany.Service;

namespace QualityCompany.TerminalCommands;

internal class TargetCommands
{
    private static readonly ACLogger _logger = new(nameof(TargetCommands));

    [TerminalCommand]
    private static TerminalCommandBuilder CreateTargetCommand()
    {
        if (!Plugin.Instance.PluginConfig.TerminalTargetCommandsEnabled) return null;

        return new TerminalCommandBuilder("target")
            .WithDescription(">TARGET <AMOUNT>\nSet a target sell requirement for the target monitor.")
            .WithText("Please enter an amount.\neg: target 2000")
            .WithSubCommand(new TerminalSubCommandBuilder("<ta>")
                .WithMessage("[companyBuyingRateWarning]Target has been set to [targetSetTo].\nMonitor Values:\n[targetMonitorValues]")
                .WithInputMatch(@"^(\d+$)$")
                .WithPreAction(input =>
                {
                    if (!int.TryParse(input, out var amount)) return false;

                    TargetManager.UpdateTarget(amount, GameNetworkManager.Instance.localPlayerController.playerUsername);

                    return true;
                })
                .WithAction(() => _logger.LogDebug("EXEC target.Action???"))
            )
            .AddTextReplacement("[targetSetTo]", () => OvertimeMonitor.targetTotalCredits.ToString())
            .AddTextReplacement("[targetMonitorValues]", OvertimeMonitor.GetText);
    }
}

