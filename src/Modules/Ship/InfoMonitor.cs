using QualityCompany.Manager.Saves;
using QualityCompany.Service;
using QualityCompany.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace QualityCompany.Modules.Ship;

internal class InfoMonitor : BaseMonitor
{
    public static InfoMonitor Instance;

    private static GameObject _depositDesk;

    internal int TargetTotalCredits => SaveManager.SaveData.TargetForSelling;
    internal int CalculatedNeededToReachTarget { get; private set; }
    internal int CalculatedOvertime { get; private set; }
    internal int CalculatedDeskTotal { get; private set; }

    protected override void PostStart()
    {
        Instance = this;
        Logger = new ModLogger(nameof(InfoMonitor));

        TextMesh.fontSize *= 0.65f;

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
            Instance.CalculatedNeededToReachTarget = CalculateSellNeeded();
            Instance.CalculatedOvertime = CalculateOvertime();
            Instance.CalculatedDeskTotal = CalculateSumOnDepositDesk();

            Instance.UpdateMonitorText(
                $"TARGET:${SaveManager.SaveData.TargetForSelling}\n" +
                $"NEEDED:${Instance.CalculatedNeededToReachTarget}\n" +
                $"OVERTIME:${Instance.CalculatedOvertime}\n" +
                $"DESK:${Instance.CalculatedDeskTotal}"
            );

            return;
        }

        var quotaStartScrap = SaveManager.SaveData.TotalShipLootAtStartForCurrentQuota;
        var isDeadlineDay = TimeOfDay.Instance.daysUntilDeadline == 0;
        var daysCompleted = SaveManager.SaveData.TotalDaysPlayedForCurrentQuota + 1;
        var lootGainedInCurrentQuota = ScrapUtils.GetShipTotalRawScrapValue() - quotaStartScrap;
        var gainedLostText = lootGainedInCurrentQuota < 0
            ? $"LOST: ${lootGainedInCurrentQuota * -1}"
            : $"GAINED: ${lootGainedInCurrentQuota}";
        Instance.UpdateMonitorText($"DAY: {(isDeadlineDay ? "DEADLINE" : daysCompleted)}\nQUOTA START: ${quotaStartScrap}\n{gainedLostText}");
    }

    internal static string GetText()
    {
        if (Instance is null) return string.Empty;

        return Instance.GetMonitorText();
    }

    private static int CalculateSellNeeded()
    {
        // if the current profit quota is more than target
        var isQuotaMoreThanTarget = GameUtils.TimeOfDay.profitQuota >= Instance.TargetTotalCredits;
        // if players have more credits than the actual target, it would show 0
        var groupCreditsMoreThanTarget = GameUtils.Terminal.groupCredits >= Instance.TargetTotalCredits;
        // if either hit, then just show remaining amount needed to just meet the quota
        if (isQuotaMoreThanTarget || groupCreditsMoreThanTarget)
        {
            return Math.Max(0, GameUtils.TimeOfDay.profitQuota - GameUtils.TimeOfDay.quotaFulfilled);
        }

        if (GameUtils.Terminal.groupCredits > Instance.TargetTotalCredits) return GameUtils.TimeOfDay.profitQuota;

        var actualNeeded = Math.Max(0, CalculateSellAmountRequired() - GameUtils.TimeOfDay.quotaFulfilled);

        // in the case where the group has a lot of credits already, calculation gets cross-wired
        // so instead, return to the remaining to reach `profitQuota` but still negating 'quotaFulfilled' (how much has been sold)
        // Thanks @throwitaway99 https://github.com/remiX-/QualityCompany/issues/4#issuecomment-1940570052
        // to confirm if this actually works
        if (actualNeeded < GameUtils.TimeOfDay.profitQuota - GameUtils.TimeOfDay.quotaFulfilled)
        {
            return Math.Max(0, GameUtils.TimeOfDay.profitQuota - GameUtils.TimeOfDay.quotaFulfilled);
        }

        return actualNeeded;
    }

    private static int CalculateSellAmountRequired()
    {
        var amountStillNeeded = Instance.TargetTotalCredits - GameUtils.Terminal.groupCredits + GameUtils.TimeOfDay.quotaFulfilled;
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
