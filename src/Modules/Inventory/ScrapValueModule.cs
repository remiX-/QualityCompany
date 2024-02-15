using QualityCompany.Modules.Core;
using TMPro;
using UnityEngine;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Modules.Inventory;

[Module(Delayed = true)]
internal class ScrapValueModule : InventoryBaseUI
{
    private static readonly Color TEXT_COLOR_ABOVE150 = new(255f / 255f, 128f / 255f, 0f / 255f, 1f); // Legendary orange??
    private static readonly Color TEXT_COLOR_ABOVE100 = new(1f, 128f / 255f, 237f / 255f, 0.75f); // Epic
    private static readonly Color TEXT_COLOR_69 = new(0f, 112f / 255f, 221f / 255f, 0.75f); // Crrtz?
    private static readonly Color TEXT_COLOR_ABOVE50 = new(30f / 255f, 1f, 0f, 0.75f); // green
    private static readonly Color TEXT_COLOR_NOOBS = new(1f, 1f, 1f, 0.75f);

    private TextMeshProUGUI _totalScrapValueText;

    public ScrapValueModule() : base(nameof(ScrapValueModule))
    { }

    [ModuleOnLoad]
    private static ScrapValueModule Spawn()
    {
        if (!Plugin.Instance.PluginConfig.InventoryShowScrapUI) return null;

        var scrapUI = new GameObject(nameof(ScrapValueModule));
        return scrapUI.AddComponent<ScrapValueModule>();
    }

    private new void Awake()
    {
        base.Awake();

        for (var i = 0; i < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; i++)
        {
            var iconFrame = HUDManager.Instance.itemSlotIconFrames[i].gameObject.transform;
            var rect = iconFrame.GetComponent<RectTransform>();
            var rectSize = rect?.sizeDelta ?? new Vector2(36, 36);
            var rectEulerAngles = rect?.eulerAngles ?? Vector3.zero;
            var zRotation = rectEulerAngles.z;
            var scrapLocalPositionDelta = GetLocalPositionDelta(zRotation, rectSize.x, rectSize.y);

            texts.Add(CreateInventoryGameObject($"qc_HUDScrapUI{i}", 10, iconFrame, scrapLocalPositionDelta));

            if (i == 0 && Plugin.Instance.PluginConfig.InventoryShowTotalScrapUI)
            {
                // Invert positioning on the first slot to be 90 degrees opposite to the current item value
                _totalScrapValueText = CreateInventoryGameObject("qc_HUDScrapUITotal", 8, iconFrame, new Vector2(scrapLocalPositionDelta.y * 3, scrapLocalPositionDelta.x * 3));
            }
        }
    }

    [ModuleOnAttach]
    private void Attach()
    {
        PlayerGrabObjectClientRpc += OnRpcUpdate;
        PlayerThrowObjectClientRpc += OnRpcUpdate;
        PlayerDiscardHeldObject += OnUpdate;
        PlayerDropAllHeldItems += HideAll;
        PlayerDeath += OnPlayerDeath;
    }

    protected override void OnUpdate(GrabbableObject currentHeldItem, int currentItemSlotIndex)
    {
        UpdateTotalScrapValue();

        if (!currentHeldItem.itemProperties.isScrap || currentHeldItem.scrapValue <= 0)
        {
            Hide(currentItemSlotIndex);
            return;
        }

        var scrapValue = currentHeldItem.scrapValue;
        UpdateItemSlotText(currentItemSlotIndex, $"${scrapValue}", GetColorForValue(scrapValue));
    }

    protected override void UpdateItemSlotText(int index, string text, Color color)
    {
        base.UpdateItemSlotText(index, text, color);
        UpdateTotalScrapValue();
    }

    protected override void Hide(int currentItemSlotIndex)
    {
        base.Hide(currentItemSlotIndex);
        UpdateTotalScrapValue();
    }

    protected void UpdateTotalScrapValue()
    {
        if (_totalScrapValueText is null) return;

        if (GameNetworkManager.Instance?.localPlayerController?.ItemSlots is null)
        {
            _logger.LogWarning("UpdateTotalScrapValue: GNM.Instance?.localPlayerController?.ItemSlots is null");
            return;
        }

        var totalScrapValue = 0;
        foreach (var slotScrap in GameNetworkManager.Instance.localPlayerController.ItemSlots)
        {
            if (slotScrap is null || !slotScrap.itemProperties.isScrap || slotScrap.scrapValue <= 0) continue;

            totalScrapValue += slotScrap.scrapValue;
        }

        _totalScrapValueText.text = $"Total: ${totalScrapValue}";
        _totalScrapValueText.enabled = totalScrapValue > 0;
        _totalScrapValueText.color = GetColorForValue(totalScrapValue);
    }

    internal static Color GetColorForValue(int value)
    {
        return value switch
        {
            > 150 => TEXT_COLOR_ABOVE150,
            > 100 => TEXT_COLOR_ABOVE100,
            69 => TEXT_COLOR_69,
            > 50 => TEXT_COLOR_ABOVE50,
            _ => TEXT_COLOR_NOOBS
        };
    }

    internal static Vector2 GetLocalPositionDelta(float parentRotationZ, float parentSizeX, float parentSizeY)
    {
        // Z - Rotation mapping for moving text "up"
        // 0    -> ++y
        // 90   -> ++x
        // 180  -> --y
        // 270  -> --x
        return parentRotationZ switch
        {
            >= 270 => new Vector2(-parentSizeX / 2f, 0f),
            >= 180 => new Vector2(0f, -parentSizeY / 2f),
            >= 90 => new Vector2(parentSizeX / 2f, 0f),
            _ => new Vector2(0f, parentSizeY / 2f)
        };
    }
}

