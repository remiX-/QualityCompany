using AdvancedCompany.Service;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Modules;

internal class ShotgunUIModule : MonoBehaviour
{
    public static ShotgunUIModule Instance { get; private set; }

    public GameObject FrameParent;
    public int ItemIndex;

    private readonly ACLogger _logger = new(nameof(ShotgunUIModule));

    private Text _text;
    private static readonly Color TEXT_COLOR_FULL = new(0f, 1f, 0f, 0.75f);
    private static readonly Color TEXT_COLOR_HALF = new(1f, 243f / 255f, 36f / 255f, 0.75f); // rgb(255, 243, 36) yellow
    private static readonly Color TEXT_COLOR_EMPTY = new(1f, 0f, 0f, 0.75f);
    

    private void Start()
    {
        Instance = this;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        _text = gameObject.AddComponent<Text>();
        _text.fontSize = 16;
        _text.font = font;
        _text.fontStyle = FontStyle.Bold;
        _text.color = TEXT_COLOR_FULL;
        _text.alignment = TextAnchor.MiddleCenter;
        _text.enabled = false;

        transform.SetParent(FrameParent.transform);
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        GameEvents.PlayerGrabObjectClientRpc += UpdateUI;
        GameEvents.PlayerDiscardHeldObject += UpdateUI;
        GameEvents.PlayerSwitchToItemSlot += UpdateUI;
        GameEvents.PlayerShotgunShoot += UpdateUI;
        GameEvents.PlayerShotgunReload += UpdateUI;
    }

    private void UpdateUI(PlayerControllerB instance)
    {
        if (instance.currentItemSlot != ItemIndex) return;

        var shotgunItem = instance.currentlyHeldObjectServer?.GetComponent<ShotgunItem>();
        if (shotgunItem is not null)
        {
            Show(shotgunItem.shellsLoaded);
        }
        else
        {
            Hide();
        }
    }

    private void Show(int shellsLoaded)
    {
        _text.enabled = true;
        _text.text = shellsLoaded.ToString();
        _text.color = shellsLoaded switch
        {
            2 => TEXT_COLOR_FULL,
            1 => TEXT_COLOR_HALF,
            _ => TEXT_COLOR_EMPTY
        };
    }

    private void Hide()
    {
        if (!_text.enabled) return;

        _text.text = string.Empty;
        _text.enabled = false;
    }
}

