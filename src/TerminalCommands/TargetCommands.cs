using AdvancedCompany.Components;
using AdvancedCompany.Manager.ShipTerminal;
using AdvancedCompany.Network;
using AdvancedCompany.Utils;

namespace AdvancedCompany.TerminalCommands;

internal class TargetCommands : ITerminalSubscriber
{
    public void Run()
    {
        AdvancedTerminal.AddCommand(
            new TerminalCommandBuilder("target")
                .WithDescription(">TARGET <AMOUNT>\nSet a target sell requirement for the target monitor.")
                .WithText("Please enter an amount.\neg: target 2000")
                .WithSubCommand(new TerminalSubCommandBuilder("<ta>")
                    .WithMessage("[companyBuyingRateWarning]Target has been set to [targetSetTo]")
                    // .WithConditions("landedAtCompany")
                    .WithInputMatch(@"(\d+$)$")
                    .WithPreAction(input =>
                    {
                        if (!int.TryParse(input, out var amount)) return false;

                        NetworkHandler.Instance.UpdateSellTargetServerRpc(amount, GameNetworkManager.Instance.localPlayerController.playerUsername);

                        return true;
                    })
                    .WithAction(() => Logger.LogDebug("EXEC target.Action???"))
                )
                .AddTextReplacement("[targetSetTo]", () => OvertimeMonitor.targetTotalCredits.ToString())
        // .WithCondition("landedAtCompany", "ERROR: Usage of this feature is only permitted within Company bounds\n\nPlease land at 71-Gordion and repeat command.", GameUtils.IsLandedOnCompany)
        );
    }
}

