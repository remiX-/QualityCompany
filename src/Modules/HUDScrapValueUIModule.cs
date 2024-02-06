using GameNetcodeStuff;
using QualityCompany.Service;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Modules;

internal class HUDScrapValueUIModule : MonoBehaviour
{
    private readonly ACLogger _logger = new(nameof(HUDScrapValueUIModule));

    private static readonly Color TEXT_COLOR_ABOVE150 = new(255f / 255f, 128f / 255f, 0f / 255f, 1f); // Legendary orange??
    private static readonly Color TEXT_COLOR_ABOVE100 = new(163f / 255f, 53f / 255f, 238f / 255f, 0.75f); // Epic
    private static readonly Color TEXT_COLOR_69 = new(0f, 112f / 255f, 221f / 255f, 0.75f); // Crrtz?
    private static readonly Color TEXT_COLOR_ABOVE50 = new(30f / 255f, 1f, 0f, 0.75f); // green
    private static readonly Color TEXT_COLOR_NOOBS = new(1f, 1f, 1f, 0.75f);

    private readonly List<TextMeshProUGUI> texts = new();

    private int totalItemSlots = 4; // game default

    // Maybe some kind of [ModuleOnSpawn] attribute?
    public static void Spawn()
    {
        if (!Plugin.Instance.PluginConfig.InventoryShowScrapUI) return;

        var scrapUI = new GameObject(nameof(HUDScrapValueUIModule));
        scrapUI.AddComponent<HUDScrapValueUIModule>();
    }

    private void Awake()
    {
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
            var text = CreateHudAndTextGameObject($"HUDScrapUI{i}", 10, iconFrame, scrapLocalPositionDelta);

            texts.Add(text);
        }

        Attach();
    }

    private void Start()
    {
        Destroy(gameObject);
    }

    // Maybe some kind of [ModuleOnAttach] attribute?
    private void Attach()
    {
        PlayerGrabObjectClientRpc += UpdateUI;
        PlayerThrowObjectClientRpc += UpdateUI;
        PlayerDiscardHeldObject += UpdateUI;
        PlayerDropAllHeldItems += HideAll;
        PlayerDeath += HideAll;
        Disconnected += Detach;
    }

    // Maybe some kind of [ModuleOnDetach] attribute?
    private void Detach(GameNetworkManager instance)
    {
        PlayerGrabObjectClientRpc -= UpdateUI;
        PlayerThrowObjectClientRpc -= UpdateUI;
        PlayerDiscardHeldObject -= UpdateUI;
        PlayerDropAllHeldItems -= HideAll;
        PlayerDeath -= HideAll;
        Disconnected -= Detach;
    }

    private void UpdateUI(PlayerControllerB instance)
    {
        if (Plugin.Instance.PluginConfig.InventoryForceUpdateAllSlotsOnDiscard)
        {
            ForceUpdateAllSlots(instance);
            return;
        }

        if (instance != GameNetworkManager.Instance.localPlayerController) return;

        if (instance.currentlyHeldObjectServer is null)
        {
            Hide(instance.currentItemSlot);
            return;
        }

        // Always show scrap value amount text
        ShowScrapValueText(instance.currentlyHeldObjectServer, instance.currentItemSlot);
    }

    private void ForceUpdateAllSlots(PlayerControllerB instance)
    {
        for (var i = 0; i < totalItemSlots; i++)
        {
            if (instance.ItemSlots[i] is null)
            {
                Hide(i);
                continue;
            }

            ShowScrapValueText(instance.ItemSlots[i], i);
        }
    }

    private void ShowScrapValueText(GrabbableObject currentHeldItem, int currentItemSlotIndex)
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

    private void Hide(int currentItemSlotIndex)
    {
        texts[currentItemSlotIndex].text = string.Empty;
        texts[currentItemSlotIndex].enabled = false;
    }

    private void HideAll(PlayerControllerB instance)
    {
        if (instance != GameNetworkManager.Instance.localPlayerController) return;

        for (var itemIndex = 0; itemIndex < totalItemSlots; itemIndex++)
        {
            Hide(itemIndex);
        }
    }

    private TextMeshProUGUI CreateHudAndTextGameObject(string gameObjectName, int fontSize, Transform parent, Vector3 localPositionDelta)
    {
        var hangerShipHeaderText = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText");
        var textObject = Instantiate(hangerShipHeaderText, parent);
        textObject.name = gameObjectName;
        textObject.transform.position = Vector3.zero;
        textObject.transform.localPosition = Vector3.zero + localPositionDelta;
        textObject.transform.localScale = Vector3.one;
        textObject.transform.rotation = Quaternion.Euler(Vector3.zero);
        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.enabled = false;

        return text;
    }
}

