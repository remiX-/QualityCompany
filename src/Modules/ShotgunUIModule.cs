using AdvancedCompany.Service;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Modules;

internal class ShotgunUIModule : MonoBehaviour
{
    public static ShotgunUIModule Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(ShotgunUIModule));

    private Text _text;
    private readonly Color TEXT_COLOR_FULL = new(0f, 1f, 0f, 1f);
    private readonly Color TEXT_COLOR_HALF = new(0f, 0f, 1f, 1f);
    private readonly Color TEXT_COLOR_EMPTY = new(1f, 0f, 0f, 1f);

    private void Awake()
    {
        Instance = this;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        _text = gameObject.AddComponent<Text>();
        _text.fontSize = 20;
        _text.font = font;
        _text.fontStyle = FontStyle.Bold;
        _text.text = "0";
        _text.color = TEXT_COLOR_FULL;
        _text.alignment = TextAnchor.MiddleCenter;
        _text.enabled = true;

        GameEvents.HudManagerStart += hudManager =>
        {
            _logger.LogMessage($"Event Sub Test: {hudManager.itemSlotIconFrames.Length}");
        };
        GameEvents.PlayerBeginGrabObject += UpdateUI;
        GameEvents.PlayerSwitchToItemSlot += UpdateUI;
    }

    private void UpdateUI(PlayerControllerB instance)
    {
        var shotgunItem = instance.currentlyHeldObjectServer?.GetComponent<ShotgunItem>();
        if (shotgunItem is not null)
        {
            Show(shotgunItem.shellsLoaded, instance.currentItemSlot);
        }
        else
        {
            Hide();
        }
    }

    private void Show(int shellsLoaded, int slotIndex)
    {
        _logger.LogMessage($"Show: shells {shellsLoaded}, index: {slotIndex}");
        _text.enabled = true;
        _text.text = shellsLoaded.ToString();
        _text.color = shellsLoaded switch
        {
            2 => TEXT_COLOR_FULL,
            1 => TEXT_COLOR_HALF,
            _ => TEXT_COLOR_EMPTY
        };

        var slotIcon = HUDManager.Instance.itemSlotIconFrames[slotIndex];
        transform.SetParent(slotIcon.gameObject.transform);
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
    }

    private void Hide()
    {
        if (!_text.enabled) return;

        _text.text = string.Empty;
        _text.enabled = false;
    }
}

