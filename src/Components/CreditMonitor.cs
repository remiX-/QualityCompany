using AdvancedCompany.Service;

namespace AdvancedCompany.Components;

internal class CreditMonitor : BaseMonitor
{
    public static CreditMonitor Instance;

    private static Terminal _terminal;

    protected override void PostStart()
    {
        Instance = this;
        _logger = new ACLogger(nameof(CreditMonitor));

        _terminal = FindObjectOfType<Terminal>();

        UpdateMonitor();
    }

    public static void UpdateMonitor()
    {
        if (_terminal is null) return;

        Instance?.UpdateMonitorText("CREDITS", _terminal.groupCredits);
    }
}