using AdvancedCompany.Service;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Modules;

internal class ScrapValueUIModule : MonoBehaviour
{
    public static ScrapValueUIModule Instance { get; private set; }

    public GameObject FrameParent;
    public int ItemIndex;

    private readonly ACLogger _logger = new(nameof(ScrapValueUIModule));

    private static readonly Color TEXT_COLOR_ABOVE150 = new(255f / 255f, 128f / 255f, 0f / 255f, 1f); // Legendary orange??
    private static readonly Color TEXT_COLOR_ABOVE100 = new(163f / 255f, 53f / 255f, 238f / 255f, 0.75f); // Epic
    private static readonly Color TEXT_COLOR_69 = new(0f, 112f / 255f, 221f / 255f, 0.75f); // Crrtz?
    private static readonly Color TEXT_COLOR_ABOVE50 = new(30f / 255f, 1f, 0f, 0.75f); // green
    private static readonly Color TEXT_COLOR_NOOBS = new(1f, 1f, 1f, 0.75f);

    private Text _text;

    private void Start()
    {
        Instance = this;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        _text = gameObject.AddComponent<Text>();
        _text.fontSize = 6; // pixels.
        _text.font = font;
        _text.fontStyle = FontStyle.Normal;
        _text.alignment = TextAnchor.MiddleCenter;
        _text.enabled = false;

        transform.SetParent(FrameParent.transform);
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero + new Vector3(-_text.fontSize * 2, 0f);
        transform.localScale = Vector3.one;

        GameEvents.PlayerGrabObjectClientRpc += UpdateUI;
        GameEvents.PlayerThrowObjectClientRpc += UpdateUI;
    }

    private void UpdateUI(PlayerControllerB instance)
    {
        if (instance != GameNetworkManager.Instance.localPlayerController) return;

        _logger.LogMessage($"UpdateUI: currentItemSlot: {instance.currentItemSlot} | ItemIndex: {ItemIndex} | currentlyHeldObjectServer: {instance.currentlyHeldObjectServer?.name ?? "null"}");
        if (instance.currentItemSlot != ItemIndex) return;

        if (instance.currentlyHeldObjectServer is null)
        {
            Hide();
        }
        else
        {
            Show(instance.currentlyHeldObjectServer);
        }
    }

    private void Show(GrabbableObject currentHeldItem)
    {
        if (currentHeldItem is null || !currentHeldItem.itemProperties.isScrap || currentHeldItem.scrapValue <= 0) return;

        _text.enabled = true;
        _text.text = "$" + currentHeldItem.scrapValue;
        _text.color = currentHeldItem.scrapValue switch
        {
            > 150 => TEXT_COLOR_ABOVE150,
            > 100 => TEXT_COLOR_ABOVE100,
            69 => TEXT_COLOR_69,
            > 50 => TEXT_COLOR_ABOVE50,
            _ => TEXT_COLOR_NOOBS
        };
    }

    private void Hide()
    {
        if (!_text.enabled) return;

        _text.text = string.Empty;
        _text.enabled = false;
    }
}

