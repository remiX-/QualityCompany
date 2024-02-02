using System.Collections;
using AdvancedCompany.Service;
using GameNetcodeStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Modules;

internal class ScrapValueUIModule : MonoBehaviour
{
    public static ScrapValueUIModule Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(ScrapValueUIModule));

    private static readonly Color TEXT_COLOR_ABOVE150 = new(192f / 255f, 100f / 255f, 147f / 255f, 1f); // purple??
    private static readonly Color TEXT_COLOR_ABOVE100 = new(0f, 1f, 0, 0.75f); // green
    private static readonly Color TEXT_COLOR_69 = new(0.8156862745f, 0.5411764706f, 0.2705882353f, 0.75f); // orange
    private static readonly Color TEXT_COLOR_ABOVE50 = new(1f, 0f, 0f, 0.75f); // red
    private static readonly Color TEXT_COLOR_NOOBS = new(1f, 1f, 1f, 0.75f);

    public GameObject FrameParent;
    public int MyItemSlotIShouldListenTo;

    private Text _text;

    private void Start()
    {
        _logger.LogDebug($"FrameParent: {FrameParent?.name ?? "null"}");
        Instance = this;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        _text = gameObject.AddComponent<Text>();
        _text.fontSize = 8;
        _text.font = font;
        _text.fontStyle = FontStyle.Normal;
        _text.text = "0";
        _text.alignment = TextAnchor.MiddleCenter;
        _text.enabled = true;

        transform.SetParent(FrameParent.transform);
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero + new Vector3(-_text.preferredHeight, 0f);
        transform.localScale = Vector3.one;

        GameEvents.PlayerBeginGrabObject += UpdateUI;
        GameEvents.PlayerDiscardHeldObject += UpdateUI;
    }

    private void UpdateUI(PlayerControllerB instance)
    {
        if (instance.currentItemSlot != MyItemSlotIShouldListenTo) return;

        if (instance.currentlyHeldObjectServer is null)
        {
            Hide();
        }
        else
        {
            Show(instance.currentlyHeldObjectServer, instance.currentItemSlot);
        }
    }

    private void Show(GrabbableObject currentHeldItem, int slotIndex)
    {
        if (currentHeldItem is null) return;

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

