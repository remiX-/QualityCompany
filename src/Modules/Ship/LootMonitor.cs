using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Service;
using QualityCompany.Utils;

namespace QualityCompany.Modules.Ship;

internal class LootMonitor : BaseMonitor
{
    public static LootMonitor Instance;

    protected override void PostStart()
    {
        Instance = this;
        Logger = new ACLogger(nameof(LootMonitor));

        UpdateMonitor();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    internal static void UpdateMonitor()
    {
        if (GameUtils.ShipGameObject == null)
        {
            Instance?.Logger.LogError("ShipGameObject is null");
            return;
        }

        Instance?.UpdateMonitorText($"LOOT:\n${ScrapUtils.GetShipSettledTotalRawScrapValue()}\nCREDITS:\n${AdvancedTerminal.Terminal.groupCredits}");
    }
}