using QualityCompany.Modules.Core;
using QualityCompany.Service;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
// ReSharper disable StringLiteralTypo

namespace QualityCompany.Modules.HUD;

[Module]
internal class PingModule : MonoBehaviour
{
    private readonly ACLogger _logger = new(nameof(PingModule));

    private static readonly Color TEXT_COLOR_ABOVE200 = new(1f, 0f, 0f, 0.8f);
    private static readonly Color TEXT_COLOR_ABOVE130 = new(1f, 128f / 255f, 237f / 255f, 0.8f);
    private static readonly Color TEXT_COLOR_ABOVE80 = new(255f / 255f, 128f / 255f, 0f / 255f, 0.8f);
    private static readonly Color TEXT_COLOR_GOOD = new(0f, 1f, 0f, 0.8f);

    private static readonly string[] ValidAnchors = { "topleft", "topright", "bottomleft", "bottomright" };

    private TextMeshProUGUI _text;

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
        var rectT = GetComponent<RectTransform>();
        var parentSize = rectT.GetParentSize();
        rectT.sizeDelta = parentSize;
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;

        _text = GetComponent<TextMeshProUGUI>();
        _text.fontSize = 10f;
        _text.text = "";

        UpdatePositionAndAlignment();

        StartCoroutine(PingCheck());
    }

    // ReSharper disable once FunctionRecursiveOnAllPaths
    private IEnumerator PingCheck()
    {
        yield return new WaitForSeconds(1f);

        _logger.LogDebug("Checking ping with host...");
        var serverClientId = NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId;
        var result = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(serverClientId);
        _logger.LogDebug($"Time? {result}ms");
        _text.text = $"{result}ms";
        _text.color = GetColorForPing(result);

        yield return new WaitForSeconds(Plugin.Instance.PluginConfig.HudPingUpdateInterval - 1f);

        StartCoroutine(PingCheck());
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

    private static Color GetColorForPing(ulong ping)
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
