using QualityCompany.Components;
using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Network;
using QualityCompany.Service;

namespace QualityCompany.TerminalCommands;

internal class TargetCommands : ITerminalSubscriber
{
    private static readonly ACLogger _logger = new(nameof(TargetCommands));

    public void Run()
    {
        AdvancedTerminal.AddCommand(
            new TerminalCommandBuilder("target")
                .WithDescription(">TARGET <AMOUNT>\nSet a target sell requirement for the target monitor.")
                .WithText("Please enter an amount.\neg: target 2000")
                .WithSubCommand(new TerminalSubCommandBuilder("<ta>")
                    .WithMessage("[companyBuyingRateWarning]Target has been set to [targetSetTo]")
                    .WithInputMatch(@"(\d+$)$")
                    .WithPreAction(input =>
                    {
                        if (!int.TryParse(input, out var amount)) return false;

                        NetworkHandler.Instance.UpdateSellTargetServerRpc(amount, GameNetworkManager.Instance.localPlayerController.playerUsername);

                        return true;
                    })
                    .WithAction(() => _logger.LogDebug("EXEC target.Action???"))
                )
                .AddTextReplacement("[targetSetTo]", () => OvertimeMonitor.targetTotalCredits.ToString())
        );
    }
}

