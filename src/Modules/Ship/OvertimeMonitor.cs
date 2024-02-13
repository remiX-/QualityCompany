using QualityCompany.Manager.Saves;
using QualityCompany.Service;
using QualityCompany.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace QualityCompany.Modules.Ship;

internal class OvertimeMonitor : BaseMonitor
{
    public static OvertimeMonitor Instance;

    private static Terminal _terminal;
    private static GameObject _depositDesk;

    private static int _targetTotalCredits = 1200;
    public static int TargetNeeded = 1200;

    protected override void PostStart()
    {
        Instance = this;
        Logger = new ACLogger(nameof(OvertimeMonitor));

        TextMesh.fontSize *= 0.65f;
        _terminal = FindFirstObjectByType<Terminal>();

        UpdateMonitor();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    internal static void UpdateMonitor()
    {
        if (Instance is null) return;

        if (GameUtils.IsOnCompany())
        {
            TargetNeeded = CalculateSellNeeded();
            var overtime = CalculateOvertime();

            Instance.UpdateMonitorText(
                $"TARGET:${SaveManager.SaveData.TargetForSelling}\n" +
                $"NEEDED:${TargetNeeded}\n" +
                $"OVERTIME:${overtime}\n" +
                $"DESK:${CalculateSumOnDepositDesk()}"
            );

            return;
        }

        var quotaStartScrap = SaveManager.SaveData.TotalShipLootAtStartForCurrentQuota;
        var isDeadlineDay = TimeOfDay.Instance.daysUntilDeadline == 0;
        var daysCompleted = SaveManager.SaveData.TotalDaysPlayedForCurrentQuota + 1;
        var lootGainedInCurrentQuota = ScrapUtils.GetShipTotalRawScrapValue() - quotaStartScrap;
        var gainedLostText = lootGainedInCurrentQuota < 0
            ? $"LOST: {lootGainedInCurrentQuota * -1}"
            : $"GAINED: {lootGainedInCurrentQuota}";
        Instance.UpdateMonitorText($"DAY: {(isDeadlineDay ? "DEADLINE" : daysCompleted)}\nQUOTA START: ${quotaStartScrap}\n${gainedLostText}");
    }

    internal static void UpdateTargetAmount(int target)
    {
        _targetTotalCredits = target;
        UpdateMonitor();
    }

    internal static string GetText()
    {
        if (Instance is null) return string.Empty;

        return Instance.GetMonitorText();
    }

    private static int CalculateSellNeeded()
    {
        // if the current profit quota is more than target
        var isQuotaMoreThanTarget = GameUtils.TimeOfDay.profitQuota >= _targetTotalCredits;
        // if players have more credits than the actual target, it would show 0
        var groupCreditsMoreThanTarget = _terminal.groupCredits >= _targetTotalCredits;
        // if either hit, then just show remaining amount needed to just meet the quota
        if (isQuotaMoreThanTarget || groupCreditsMoreThanTarget)
        {
            return Math.Max(0, GameUtils.TimeOfDay.profitQuota - GameUtils.TimeOfDay.quotaFulfilled);
        }

        if (_terminal.groupCredits > _targetTotalCredits) return GameUtils.TimeOfDay.profitQuota;

        return Math.Max(0, CalculateSellAmountRequired() - GameUtils.TimeOfDay.quotaFulfilled);
    }

    private static int CalculateSellAmountRequired()
    {
        var amountStillNeeded = _targetTotalCredits - _terminal.groupCredits + GameUtils.TimeOfDay.quotaFulfilled;
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
        _depositDesk = GameObject.Find("/DepositCounter/DoorAndHookAnim/InteractCube");
        if (_depositDesk == null)
        {
            return 0;
        }

        var deskScrapItems = _depositDesk.GetComponentsInChildren<GrabbableObject>().Where(item => item.itemProperties.isScrap);
        return ScrapUtils.SumScrapListSellValue(deskScrapItems);
    }
}
