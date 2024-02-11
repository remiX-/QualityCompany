using QualityCompany.Modules.Core;
using QualityCompany.Network;
using QualityCompany.Service;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
// ReSharper disable StringLiteralTypo

namespace QualityCompany.Modules.HUD;

[Module]
internal class PingModule : MonoBehaviour
{
    public static PingModule Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(PingModule));

    private static readonly Color TEXT_COLOR_ABOVE200 = new(1f, 0f, 0f, 0.8f);
    private static readonly Color TEXT_COLOR_ABOVE130 = new(1f, 128f / 255f, 237f / 255f, 0.8f);
    private static readonly Color TEXT_COLOR_ABOVE80 = new(255f / 255f, 128f / 255f, 0f / 255f, 0.8f);
    private static readonly Color TEXT_COLOR_GOOD = new(0f, 1f, 0f, 0.8f);

    private static readonly string[] ValidAnchors = { "topleft", "topright", "bottomleft", "bottomright" };

    private TextMeshProUGUI _text;
    private DateTime pingTime;

    [ModuleOnLoad]
    private static PingModule Spawn()
    {
        if (!Plugin.Instance.PluginConfig.HudPingEnabled) return null;

        var hudTimeNumber = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/ProfitQuota/Container/Box/TimeNumber");
        var ui = Instantiate(hudTimeNumber, HUDManager.Instance.HUDContainer.transform);
        ui.name = "qc_ping";
        ui.transform.position = Vector3.zero;
        ui.transform.localPosition = Vector3.zero;
        ui.transform.localScale = Vector3.one;
        return ui.AddComponent<PingModule>();
    }

    private void Awake()
    {
        Instance = this;

        var rectT = GetComponent<RectTransform>();
        var parentSize = rectT.GetParentSize();
        rectT.sizeDelta = parentSize;
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;

        _text = GetComponent<TextMeshProUGUI>();
        _text.fontSize = 10f;
        _text.text = "";

        UpdatePositionAndAlignment();

        // StartCoroutine(PingCheck());
        InitialLatencyCheck();
    }

    private void UpdatePositionAndAlignment()
    {
        var anchor = Plugin.Instance.PluginConfig.HudPingAnchor;
        var padding = Plugin.Instance.PluginConfig.HudPingAnchorPadding;

        if (!ValidAnchors.Contains(anchor))
        {
            _logger.LogWarning($"Invalid anchor for ping hud: {anchor}, defaulting to BottomLeft");
        }

        _text.alignment = anchor switch
        {
            "topleft" => TextAlignmentOptions.TopLeft,
            "topright" => TextAlignmentOptions.TopRight,
            "bottomright" => TextAlignmentOptions.BottomRight,
            _ => TextAlignmentOptions.BottomLeft
        };

        transform.localPosition = anchor switch
        {
            "topleft" => new Vector2(padding.x, -padding.y),
            "topright" => new Vector2(-padding.x, -padding.y),
            "bottomright" => new Vector2(-padding.x, padding.y),
            _ => new Vector2(padding.x, padding.y)
        };
    }

    // ReSharper disable once FunctionRecursiveOnAllPaths
    private void InitialLatencyCheck()
    {
        _logger.LogDebug("Checking ping with host...");

        pingTime = DateTime.Now;
        LatencyHandler.Instance.PingServerRpc();
    }

    private IEnumerator DeferredLatencyCheck(float delta)
    {
        var waitDelta = Plugin.Instance.PluginConfig.HudPingUpdateInterval - delta;
        waitDelta = waitDelta <= 0.2f ? 1f : waitDelta;
        _logger.LogDebug($"Checking ping with host in {waitDelta}s  ...");

        yield return new WaitForSeconds(waitDelta);

        pingTime = DateTime.Now;
        LatencyHandler.Instance.PingServerRpc();
    }

    internal void UpdateLatency()
    {
        var pingDelta = (DateTime.Now - pingTime).TotalMilliseconds;
        var pingAverage = pingDelta / 2;

        _logger.LogDebug($" latency? {pingDelta} / 2 = {pingAverage}ms");
        _text.text = $"{pingAverage}ms";
        _text.color = GetColorForPing(pingAverage);

        StartCoroutine(DeferredLatencyCheck((float)pingDelta));
    }

    private static Color GetColorForPing(double ping)
    {
        return ping switch
        {
            > 200 => TEXT_COLOR_ABOVE200,
            > 130 => TEXT_COLOR_ABOVE130,
            > 80 => TEXT_COLOR_ABOVE80,
            _ => TEXT_COLOR_GOOD
        };
    }
}
