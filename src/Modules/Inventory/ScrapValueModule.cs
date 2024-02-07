using QualityCompany.Modules.Core;
using UnityEngine;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Modules.Inventory;

[Module(Delayed = true)]
internal class ScrapValueModule : InventoryBaseUI
{
    private static readonly Color TEXT_COLOR_ABOVE150 = new(255f / 255f, 128f / 255f, 0f / 255f, 1f); // Legendary orange??
    private static readonly Color TEXT_COLOR_ABOVE100 = new(163f / 255f, 53f / 255f, 238f / 255f, 0.75f); // Epic
    private static readonly Color TEXT_COLOR_69 = new(0f, 112f / 255f, 221f / 255f, 0.75f); // Crrtz?
    private static readonly Color TEXT_COLOR_ABOVE50 = new(30f / 255f, 1f, 0f, 0.75f); // green
    private static readonly Color TEXT_COLOR_NOOBS = new(1f, 1f, 1f, 0.75f);

    public ScrapValueModule() : base(nameof(ScrapValueModule))
    { }

    [ModuleOnStart]
    private static ScrapValueModule Spawn()
    {
        if (!Plugin.Instance.PluginConfig.InventoryShowScrapUI) return null;

        var scrapUI = new GameObject(nameof(ScrapValueModule));
        return scrapUI.AddComponent<ScrapValueModule>();
    }

    private new void Awake()
    {
        base.Awake();
        _logger.LogDebug("Module.Awake");
        transform.SetParent(HUDManager.Instance.HUDContainer.transform);
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        totalItemSlots = HUDManager.Instance.itemSlotIconFrames.Length;

        for (var i = 0; i < totalItemSlots; i++)
        {
            var iconFrame = HUDManager.Instance.itemSlotIconFrames[i].gameObject.transform;
            var rect = iconFrame.GetComponent<RectTransform>();
            var rectSize = rect?.sizeDelta ?? new Vector2(36, 36);
            var rectEulerAngles = rect?.eulerAngles ?? Vector3.zero;
            var zRotation = rectEulerAngles.z;
            // Z - Rotation mapping for moving text "up"
            // 0    -> ++y
            // 90   -> ++x
            // 180  -> --y
            // 270  -> --x
            var scrapLocalPositionDelta = zRotation switch
            {
                >= 270 => new Vector2(-rectSize.x / 2f, 0f),
                >= 180 => new Vector2(0f, -rectSize.y / 2f),
                >= 90 => new Vector2(rectSize.x / 2f, 0f),
                _ => new Vector2(0f, rectSize.y / 2f)
            };

            CreateInventoryGameObject($"HUDScrapUI{i}", 10, iconFrame, scrapLocalPositionDelta);
        }
    }

    [ModuleOnAttach]
    private void Attach()
    {
        _logger.LogDebug($"Attach {nameof(ScrapValueModule)}");
        PlayerGrabObjectClientRpc += OnRpcUpdate;
        PlayerThrowObjectClientRpc += OnRpcUpdate;
        PlayerDiscardHeldObject += OnUpdate;
        PlayerDropAllHeldItems += HideAll;
        PlayerDeath += HideAll;
    }

    [ModuleOnDetach]
    private void Detach()
    {
        _logger.LogDebug($"Detach {nameof(ScrapValueModule)}");
        PlayerGrabObjectClientRpc -= OnRpcUpdate;
        PlayerThrowObjectClientRpc -= OnRpcUpdate;
        PlayerDiscardHeldObject -= OnUpdate;
        PlayerDropAllHeldItems -= HideAll;
        PlayerDeath -= HideAll;
    }

    protected override void OnUpdate(GrabbableObject currentHeldItem, int currentItemSlotIndex)
    {
        if (!currentHeldItem.itemProperties.isScrap || currentHeldItem.scrapValue <= 0)
        {
            Hide(currentItemSlotIndex);
            return;
        }

        var text = texts[currentItemSlotIndex];

        text.enabled = true;
        text.text = "$" + currentHeldItem.scrapValue;
        text.color = currentHeldItem.scrapValue switch
        {
            > 150 => TEXT_COLOR_ABOVE150,
            > 100 => TEXT_COLOR_ABOVE100,
            69 => TEXT_COLOR_69,
            > 50 => TEXT_COLOR_ABOVE50,
            _ => TEXT_COLOR_NOOBS
        };
    }
}

