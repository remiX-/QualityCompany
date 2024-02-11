using QualityCompany.Modules.Core;
using QualityCompany.Network;
using QualityCompany.Service;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
// ReSharper disable StringLiteralTypo

namespace QualityCompany.Modules.HUD;

[Module(Delayed = true)]
internal class LatencyModule : MonoBehaviour
{
    public static LatencyModule Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(LatencyModule));

    private static readonly Color TEXT_COLOR_ABOVE200 = new(1f, 0f, 0f, 0.8f);
    private static readonly Color TEXT_COLOR_ABOVE130 = new(1f, 128f / 255f, 237f / 255f, 0.8f);
    private static readonly Color TEXT_COLOR_ABOVE80 = new(255f / 255f, 128f / 255f, 0f / 255f, 0.8f);
    private static readonly Color TEXT_COLOR_GOOD = new(0f, 1f, 0f, 0.8f);

    private static readonly string[] ValidAnchors = { "topleft", "topright", "bottomleft", "bottomright" };

    private TextMeshProUGUI _text;
    private DateTime _latencyTime;

    [ModuleOnLoad]
    private static LatencyModule Spawn()
    {
        if (!Plugin.Instance.PluginConfig.HudPingEnabled || NetworkManager.Singleton.IsHost) return null;

        var hudTimeNumber = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/ProfitQuota/Container/Box/TimeNumber");
        var ui = Instantiate(hudTimeNumber, HUDManager.Instance.HUDContainer.transform);
        ui.name = "qc_latency";
        ui.transform.position = Vector3.zero;
        ui.transform.localPosition = Vector3.zero;
        ui.transform.localScale = Vector3.one;
        return ui.AddComponent<LatencyModule>();
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

        InitialLatencyCheck();
    }

    private void UpdatePositionAndAlignment()
    {
        var anchor = Plugin.Instance.PluginConfig.HudPingAnchor.ToLower();
        var paddingX = Plugin.Instance.PluginConfig.HudPingHorizontalPadding;
        var paddingY = Plugin.Instance.PluginConfig.HudPingVerticalPadding;

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
            "topleft" => new Vector2(paddingX, -paddingY),
            "topright" => new Vector2(-paddingX, -paddingY),
            "bottomright" => new Vector2(-paddingX, paddingY),
            _ => new Vector2(paddingX, paddingY)
        };
    }

    private void InitialLatencyCheck()
    {
        _latencyTime = DateTime.Now;
        LatencyHandler.Instance.PingServerRpc(GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    private IEnumerator DeferredLatencyCheck(float delta)
    {
        var waitDelta = Plugin.Instance.PluginConfig.HudPingUpdateInterval - delta / 1000f;
        waitDelta = waitDelta <= 1f ? 1f : waitDelta;

        yield return new WaitForSeconds(waitDelta);

        _latencyTime = DateTime.Now;
        LatencyHandler.Instance.PingServerRpc(GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    internal void UpdateLatency()
    {
        var latency = (DateTime.Now - _latencyTime).TotalMilliseconds;
        var latencyRoundedUp = (int)Math.Ceiling(latency);
        _text.text = $"{latencyRoundedUp}ms";
        _text.color = GetColorForPing(latencyRoundedUp);

        StartCoroutine(DeferredLatencyCheck((float)latency));
    }

    private static Color GetColorForPing(int ping)
    {
        return ping switch
        {
            >= 200 => TEXT_COLOR_ABOVE200,
            >= 130 => TEXT_COLOR_ABOVE130,
            >= 80 => TEXT_COLOR_ABOVE80,
            _ => TEXT_COLOR_GOOD
        };
    }
}
