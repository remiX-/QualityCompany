using AdvancedCompany.Components;
using AdvancedCompany.Game;
using AdvancedCompany.Manager;

namespace AdvancedCompany.TerminalCommands;

internal class TargetCommands : ITerminalSubscriber
{
    public void Run()
    {
        TerminalManager.AddCommand(
            new TerminalCommandBuilder("target")
                .WithDescription(">TARGET <AMOUNT>\nSet a target sell requirement for the target monitor.")
                .WithText("Please enter an amount.\neg: target 2000")
                .WithSubCommand(new TerminalSubCommandBuilder("<ta>")
                    .WithMessage("[companyBuyingRateWarning]Target has been set to [targetSetTo]")
                    .WithConditions("landedAtCompany")
                    .WithInputMatch(@"(\d+$)$")
                    .WithPreAction(input =>
                    {
                        if (!int.TryParse(input, out var amount)) return false;

                        OvertimeMonitor.targetTotalCredits = amount;
                        OvertimeMonitor.UpdateMonitor();

                        return true;
                    })
                    .WithAction(() => Logger.LogDebug("EXEC target.Action???"))
                )
                .AddTextReplacement("[targetSetTo]", () => OvertimeMonitor.targetTotalCredits.ToString())
                // .WithCondition("landedAtCompany", "ERROR: Usage of this feature is only permitted within Company bounds\n\nPlease land at 71-Gordion and repeat command.", GameUtils.IsLandedOnCompany)
        );
    }
}

