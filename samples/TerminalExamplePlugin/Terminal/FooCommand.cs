using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Utils;
using System;
using System.Linq;

namespace TerminalExamplePlugin.Terminal;

internal class FooCommand
{
    [TerminalCommand]
    private static TerminalCommandBuilder FooBar()
    {
        return new TerminalCommandBuilder("foo1")
            .WithAction(() => "bar!");
    }

    [TerminalCommand]
    private static TerminalCommandBuilder FooBarWithBasicSubCommand()
    {
        return new TerminalCommandBuilder("foo2")
            .WithSubCommand(new TerminalSubCommandBuilder("bar")
                .WithMessage("Did you just foobar me?")
                .WithPreAction(() =>
                {
                    // do any work here that needs to occur before the primary actions, primary used for calculations & conditions (see FooBarWithInputCommand)
                    // you can then use these in the confirmation window, for e.g. listing a bunch of scrap for sell
                })
                .WithAction(() =>
                {
                    // Some action to perform
                    // Can also do RPC methods here if your mod supports it
                })
            );
    }

    [TerminalCommand]
    private static TerminalCommandBuilder FooBarWithInputCommand()
    {
        var fooAmount = 0;
        return new TerminalCommandBuilder("foo3")
            .WithText("Please enter an amount.")
            .WithSubCommand(new TerminalSubCommandBuilder("<foo_amount>")
                .WithMessage("Are you sure you want [x_foo_amount] foos? nice")
                .EnableConfirmDeny(confirmMessage: "Here's your {{you_can_use_any_unique_replacements}} lol:\n\n[all_the_foos]")
                .WithConditions("moreThanOneFooPls")
                .WithInputMatch(@"^(\d+)$")
                .WithPreAction(input =>
                {
                    fooAmount = Convert.ToInt32(input);

                    // sanity tbh, this may not even be needed but needs to be "conditioned" somewhere.
                    // could even be a 
                    if (fooAmount <= 0) return false;

                    return true;
                })
                .WithAction(() =>
                {
                    // no actual host / client / rpc action in this example
                    // but for eg lets display a Toast just for this client
                    HudUtils.DisplayNotification($"Wow! {fooAmount} foos :)");
                })
            )
            .AddTextReplacement("[x_foo_amount]", () => fooAmount.ToString())
            .AddTextReplacement("{{you_can_use_any_unique_replacements}}", () => "foos")
            .AddTextReplacement("[all_the_foos]", () => Enumerable.Repeat("foo", fooAmount).Aggregate((first, second) => $"{first}\n{second}"))
            .WithCondition("moreThanOneFooPls", "Come on, > 1 foo pls.", () => fooAmount > 1);
    }
}