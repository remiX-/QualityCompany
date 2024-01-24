using TMPro;
using UnityEngine;

namespace AdvancedCompany.Components;

public class TimeMonitor : MonoBehaviour
{
    private static TextMeshProUGUI _textMesh;

    private static TextMeshProUGUI _timeMesh;

    public void Start()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        _timeMesh = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/ProfitQuota/Container/Box/TimeNumber").GetComponent<TextMeshProUGUI>();
        _textMesh.text = "TIME:\n7:30\nAM";
    }

    public static void UpdateMonitor()
    {
        _textMesh.text = "TIME:\n" + ((TMP_Text)_timeMesh).text;
    }
}
