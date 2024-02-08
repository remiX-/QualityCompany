using QualityCompany.Modules.Core;
using TMPro;
using UnityEngine;
using static QualityCompany.Service.GameEvents;

namespace ModuleExamplePlugin.Modules;

[Module]
internal class TimeHudModule : MonoBehaviour
{
    private static TextMeshProUGUI _timeMesh;

    private TextMeshProUGUI text;

    protected void Awake()
    {
        _timeMesh = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/ProfitQuota/Container/Box/TimeNumber").GetComponent<TextMeshProUGUI>();

        transform.SetParent(_timeMesh.transform);
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero + new Vector3(0f, 75f);
        transform.localScale = Vector3.one;

        text = gameObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = 20;
        text.alignment = TextAlignmentOptions.Center;
        text.enabled = true;
    }

    [ModuleOnStart]
    private static TimeHudModule Spawn()
    {
        var go = new GameObject(nameof(TimeHudModule));
        return go.AddComponent<TimeHudModule>();
    }

    [ModuleOnAttach]
    private void Attach()
    {
        ModuleExamplePlugin.Log.LogDebug($"Attach");
        GameTimeUpdate += UpdateTime;
    }

    [ModuleOnDetach]
    private void Detach()
    {
        ModuleExamplePlugin.Log.LogDebug($"Detach");
        GameTimeUpdate -= UpdateTime;
    }

    private void UpdateTime()
    {
        ModuleExamplePlugin.Log.LogDebug($"UpdateTime {_timeMesh.text}");
        text.text = _timeMesh.text;
    }
}