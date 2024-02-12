using QualityCompany.Network;
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

    public static int targetTotalCredits = 1200;
    public static int targetNeeded = 1200;

    protected override void PostStart()
    {
        Instance = this;
        _logger = new ACLogger(nameof(OvertimeMonitor));

        _textMesh.fontSize *= 0.65f;
        _terminal = FindFirstObjectByType<Terminal>();

        targetTotalCredits = CompanyNetworkHandler.Instance.SaveData.TargetForSelling;

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
            targetNeeded = CalculateSellNeeded();
            var overtime = CalculateOvertime();

            Instance.UpdateMonitorText($"TARGET:${targetTotalCredits}\nNEEDED:${targetNeeded}\nOVERTIME:${overtime}\nDESK:${CalculateSumOnDepositDesk()}");

            return;
        }

        if (!Plugin.Instance.PluginConfig.NetworkingEnabled)
        {
            Instance.UpdateMonitorText("DISABLED");
            return;
        }

        var quotaStartScrap = CompanyNetworkHandler.Instance.SaveData.TotalLootValue;
        var daysCompleted = CompanyNetworkHandler.Instance.SaveData.TotalDaysPlayedForCurrentQuota + 1;
        var lootGainedInCurrentQuota = ScrapUtils.GetShipTotalRawScrapValue() - quotaStartScrap;
        Instance.UpdateMonitorText($"On day: {daysCompleted}\nQuota start: ${quotaStartScrap}\nGained: ${lootGainedInCurrentQuota}");
    }

    internal static string GetText()
    {
        if (Instance is null) return string.Empty;

        return Instance.GetMonitorText();
    }

    private static int CalculateSellNeeded()
    {
        // if the current profit quota is more than target
        var isQuotaMoreThanTarget = GameUtils.TimeOfDay.profitQuota >= targetTotalCredits;
        // if players have more credits than the actual target, it would show 0
        var groupCreditsMoreThanTarget = _terminal.groupCredits >= targetTotalCredits;
        // if either hit, then just show remaining amount needed to just meet the quota
        if (isQuotaMoreThanTarget || groupCreditsMoreThanTarget)
        {
            return Math.Max(0, GameUtils.TimeOfDay.profitQuota - GameUtils.TimeOfDay.quotaFulfilled);
        }

        if (_terminal.groupCredits > targetTotalCredits) return GameUtils.TimeOfDay.profitQuota;

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
        _depositDesk = GameObject.Find("/DepositCounter/DoorAndHookAnim/InteractCube");
        if (_depositDesk == null)
        {
            return 0;
        }

        var deskScrapItems = _depositDesk.GetComponentsInChildren<GrabbableObject>().Where(item => item.itemProperties.isScrap);
        return ScrapUtils.SumScrapListSellValue(deskScrapItems);
    }
}
