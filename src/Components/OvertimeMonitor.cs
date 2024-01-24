using System;
using System.Linq;
using AdvancedCompany.Game;
using AdvancedCompany.Scrap;
using UnityEngine;

namespace AdvancedCompany.Components;

public class OvertimeMonitor : BaseMonitor
{
    public static OvertimeMonitor Instance;

    private static Terminal _terminal;
    private static GameObject _depositDesk;

    public static int targetTotalCredits = 1200;
    private static float sellNeededForCurrentQuota;

    protected override void PostStart()
    {
        Instance = this;

        _textMesh.fontSize *= 0.65f;
        _terminal = FindObjectOfType<Terminal>();

        SetupTerminalCommands();

        UpdateMonitor();
    }

    private static void SetupTerminalCommands()
    {
        // TerminalParsedSentence += TextSubmitted;
    }

    // private static void TextSubmitted(object sender, TerminalParseSentenceEventArgs e)
    // {
    //     var input = e.SubmittedText;
    //     Logger.LogMessage($"OvertimeMonitor.TextSubmitted: {e.SubmittedText} Node Returned: {e.ReturnedNode}");
    //
    //     if (input is null || input.Length == 0) return;
    //
    //     var inputArray = input.Split(" ");
    //     var command = inputArray[0].ToLower();
    //     if (command != "target" || inputArray.Length == 1) return;
    //
    //     var amountArg = inputArray[1];
    //     Logger.LogMessage($"Input checker: |{input}|");
    //
    //     if (!int.TryParse(amountArg, out var amount)) return;
    //     
    //     targetTotalCredits = amount;
    //
    //     UpdateMonitor();
    // }

    public static void UpdateMonitor()
    {
        if (!GameUtils.IsOnCompany())
        {
            Instance.UpdateMonitorText(GameUtils.TimeOfDay.daysUntilDeadline == 0
                ? "Company time boiz"
                : $"Finish {GameUtils.TimeOfDay.daysUntilDeadline} more days");

            return;
        }
        _depositDesk = GameObject.Find("/DepositCounter/DoorAndHookAnim/InteractCube");

        var stillNeeded = CalculateSellNeeded();
        var overtime = CalculateOvertime();

        Instance.UpdateMonitorText($"TARGET:${targetTotalCredits}\nNEEDED:${stillNeeded}\nOVERTIME:${overtime}\nDESK:${CalculateSumOnDepositDesk()}");
    }

    // TODO maybe int rather, but testing
    private static float CalculateSellNeeded()
    {
        sellNeededForCurrentQuota = CalculateSellAmountRequired();

        return Math.Max(0, sellNeededForCurrentQuota - GameUtils.TimeOfDay.quotaFulfilled);
    }

    private static int CalculateSellAmountRequired()
    {
        if (GameUtils.TimeOfDay.profitQuota >= targetTotalCredits)
        {
            return Math.Max(0, GameUtils.TimeOfDay.profitQuota - GameUtils.TimeOfDay.quotaFulfilled);
        }

        var amountStillNeeded = targetTotalCredits - _terminal.groupCredits + GameUtils.TimeOfDay.quotaFulfilled;
        var deadlineDaysDifference = 15 * (GameUtils.TimeOfDay.daysUntilDeadline - 1);
        return (int)Math.Ceiling(5 * (amountStillNeeded - deadlineDaysDifference + (float)GameUtils.TimeOfDay.profitQuota / 5) / 6);
    }

    private static int CalculateOvertime()
    {
        // If sold is still < current profitQuota, no point in calculating
        if (GameUtils.TimeOfDay.quotaFulfilled < GameUtils.TimeOfDay.profitQuota) return 0;

        var amountLeftToFulfilQuota = GameUtils.TimeOfDay.quotaFulfilled - GameUtils.TimeOfDay.profitQuota;
        var overtime = amountLeftToFulfilQuota / 5 + 15 * (GameUtils.TimeOfDay.daysUntilDeadline - 1);
        return overtime < 0 ? 0 : overtime;
    }

    private static int CalculateSumOnDepositDesk()
    {
        if (_depositDesk == null)
        {
            return 0;
        }

        var deskScrapItems = _depositDesk.GetComponentsInChildren<GrabbableObject>().Where(item => item.itemProperties.isScrap);
        return ScrapHelpers.SumScrapListSellValue(deskScrapItems);
    }
}
