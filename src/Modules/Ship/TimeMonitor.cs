using QualityCompany.Service;
using TMPro;
using UnityEngine;

namespace QualityCompany.Modules.Ship;

internal class TimeMonitor : BaseMonitor
{
    public static TimeMonitor Instance;

    private static TextMeshProUGUI _timeMesh;

    protected override void PostStart()
    {
        Instance = this;

        Logger = new ModLogger(nameof(TimeMonitor));
        _timeMesh = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/ProfitQuota/Container/Box/TimeNumber").GetComponent<TextMeshProUGUI>();
        TextMesh.text = "TIME:\n7:30\nAM";
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    internal static void UpdateMonitor()
    {
        Instance?.UpdateMonitorText($"TIME:\n{_timeMesh.text}");
    }
}
