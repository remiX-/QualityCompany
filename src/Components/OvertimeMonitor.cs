using QualityCompany.Network;
using QualityCompany.Service;
using QualityCompany.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace QualityCompany.Components;

internal class OvertimeMonitor : BaseMonitor
{
    public static OvertimeMonitor Instance;

    private static Terminal _terminal;
    private static GameObject _depositDesk;

    public static int targetTotalCredits = 1200;

    protected override void PostStart()
    {
        Instance = this;
        _logger = new ACLogger(nameof(OvertimeMonitor));

        _textMesh.fontSize *= 0.65f;
        _terminal = FindObjectOfType<Terminal>();

        UpdateMonitor();
    }

    public static void UpdateMonitor()
    {
        if (Instance is null) return;

        if (!GameUtils.IsOnCompany())
        {
            var quotaStartScrap = CompanyNetworkHandler.Instance.SaveData.TotalLootValue;
            var daysCompleted = CompanyNetworkHandler.Instance.SaveData.TotalDaysPlayedForCurrentQuota + 1;
            var lootGainedInCurrentQuota = ScrapUtils.GetShipTotalRawScrapValue() - quotaStartScrap;
            Instance.UpdateMonitorText($"On day: {daysCompleted}\nQuota start: ${quotaStartScrap}\nGained: ${lootGainedInCurrentQuota}");

            return;
        }
        _depositDesk = GameObject.Find("/DepositCounter/DoorAndHookAnim/InteractCube");

        var stillNeeded = CalculateSellNeeded();
        var overtime = CalculateOvertime();

        Instance.UpdateMonitorText($"TARGET:${targetTotalCredits}\nNEEDED:${stillNeeded}\nOVERTIME:${overtime}\nDESK:${CalculateSumOnDepositDesk()}");
    }

    private static int CalculateSellNeeded()
    {
        if (GameUtils.TimeOfDay.profitQuota >= targetTotalCredits)
        {
            return Math.Max(0, GameUtils.TimeOfDay.profitQuota - GameUtils.TimeOfDay.quotaFulfilled);
        }

        return Math.Max(0, CalculateSellAmountRequired() - GameUtils.TimeOfDay.quotaFulfilled);
    }

    private static int CalculateSellAmountRequired()
    {
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
        return ScrapUtils.SumScrapListSellValue(deskScrapItems);
    }
}
