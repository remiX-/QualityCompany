using System.Linq;
using AdvancedCompany.Game;

namespace AdvancedCompany.Components;

public class LootMonitor : BaseMonitor
{
    public static LootMonitor Instance;

    protected override void PostStart()
    {
        Instance = this;

        UpdateMonitor();
    }

    public static void UpdateMonitor()
    {
        if (GameUtils.ShipGameObject == null)
        {
            Logger.LogError("ShipGameObject is null");
            return;
        }

        var num = CalculateShipScrapLoot();
        Instance?.UpdateMonitorText("LOOT", num);
    }

    private static int CalculateShipScrapLoot()
    {
        return GameUtils.ShipGameObject.GetComponentsInChildren<GrabbableObject>()
            .Where(item => item.itemProperties.isScrap && !item.isHeld && !item.isPocketed)
            .Sum(item => item.scrapValue);
        // return (from x in _ship.GetComponentsInChildren<GrabbableObject>()
        //     where x.itemProperties.isScrap && !x.isPocketed && !x.isHeld
        //     select x).Sum(x => x.scrapValue);
    }
}