using GameNetcodeStuff;
using QualityCompany.Service;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Modules;

internal class HUDExtensionModule : MonoBehaviour
{
    public static HUDExtensionModule Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(HUDExtensionModule));

    private static readonly Color TEXT_COLOR_ABOVE150 = new(255f / 255f, 128f / 255f, 0f / 255f, 1f); // Legendary orange??
    private static readonly Color TEXT_COLOR_ABOVE100 = new(163f / 255f, 53f / 255f, 238f / 255f, 0.75f); // Epic
    private static readonly Color TEXT_COLOR_69 = new(0f, 112f / 255f, 221f / 255f, 0.75f); // Crrtz?
    private static readonly Color TEXT_COLOR_ABOVE50 = new(30f / 255f, 1f, 0f, 0.75f); // green
    private static readonly Color TEXT_COLOR_NOOBS = new(1f, 1f, 1f, 0.75f);

    private static readonly Color TEXT_COLOR_FULL = new(0f, 1f, 0f, 0.75f);
    private static readonly Color TEXT_COLOR_HALF = new(1f, 243f / 255f, 36f / 255f, 0.75f); // rgb(255, 243, 36) yellow
    private static readonly Color TEXT_COLOR_EMPTY = new(1f, 0f, 0f, 0.75f);

    private readonly List<Text> scrapTexts = new();
    private readonly List<Text> shotgunTexts = new();

    private int totalItemSlots = 4; // game default

    // Maybe some kind of [ModuleOnSpawn] attribute?
    public static void Spawn()
    {
        var scrapUI = new GameObject(nameof(HUDExtensionModule));
        scrapUI.AddComponent<HUDExtensionModule>();
    }

    private void Awake()
    {
        Instance = this;

        transform.SetParent(HUDManager.Instance.HUDContainer.transform);
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        var scrapFontSize = 6;
        totalItemSlots = HUDManager.Instance.itemSlotIconFrames.Length;

        for (var i = 0; i < totalItemSlots; i++)
        {
            var iconFrame = HUDManager.Instance.itemSlotIconFrames[i].gameObject.transform;
            var rect = iconFrame.GetComponent<UnityEngine.RectTransform>();
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
            var text = CreateHudAndTextGameObject($"HUDScrapUI{i}", scrapFontSize, iconFrame, TextAnchor.MiddleCenter, scrapLocalPositionDelta);
            var shotgunText = CreateHudAndTextGameObject($"HUDShotgunAmmoUI{i}", 16, HUDManager.Instance.itemSlotIconFrames[i].gameObject.transform, Vector3.zero);

            scrapTexts.Add(text);
            shotgunTexts.Add(shotgunText);
        }

        Attach();
    }

    // Maybe some kind of [ModuleOnAttach] attribute?
    private void Attach()
    {
        // deposit at desk
        // dying
        // depositing into shopping cart (LGU)?
        PlayerGrabObjectClientRpc += UpdateUI;
        PlayerThrowObjectClientRpc += UpdateUI;
        PlayerDiscardHeldObject += UpdateUI;
        PlayerDropAllHeldItems += HideAll;
        PlayerDeath += HideAll;
        PlayerShotgunShoot += UpdateUI;
        PlayerShotgunReload += UpdateUI;
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
        PlayerShotgunShoot -= UpdateUI;
        PlayerShotgunReload -= UpdateUI;
        Disconnected -= Detach;
    }

    private void UpdateUI(PlayerControllerB instance)
    {
        if (instance != GameNetworkManager.Instance.localPlayerController) return;

        if (instance.currentlyHeldObjectServer is null)
        {
            Hide(instance.currentItemSlot);
            return;
        }

        // Always show scrap value amount text
        ShowScrapValueText(instance.currentlyHeldObjectServer, instance.currentItemSlot);

        // Show shotgun ammo counter UI if currently held item has a ShotgunItem component
        var shotgunItem = instance.currentlyHeldObjectServer.GetComponent<ShotgunItem>();
        if (shotgunItem is not null)
        {
            ShowShotgunAmmoText(instance.currentItemSlot, shotgunItem.shellsLoaded);
        }
    }

    private void ShowScrapValueText(GrabbableObject currentHeldItem, int currentItemSlotIndex)
    {
        if (!currentHeldItem.itemProperties.isScrap || currentHeldItem.scrapValue <= 0) return;

        var text = scrapTexts[currentItemSlotIndex];

        // ▲
        text.enabled = true;
        text.text = "\u25b2\u258e" + currentHeldItem.scrapValue;
        text.color = currentHeldItem.scrapValue switch
        {
            > 150 => TEXT_COLOR_ABOVE150,
            > 100 => TEXT_COLOR_ABOVE100,
            69 => TEXT_COLOR_69,
            > 50 => TEXT_COLOR_ABOVE50,
            _ => TEXT_COLOR_NOOBS
        };
    }

    private void ShowShotgunAmmoText(int currentItemSlotIndex, int shellsLoaded)
    {
        var text = shotgunTexts[currentItemSlotIndex];
        text.enabled = true;
        text.text = shellsLoaded.ToString();
        text.color = shellsLoaded switch
        {
            2 => TEXT_COLOR_FULL,
            1 => TEXT_COLOR_HALF,
            _ => TEXT_COLOR_EMPTY
        };
    }

    private void Hide(int currentItemSlotIndex)
    {
        scrapTexts[currentItemSlotIndex].text = string.Empty;
        scrapTexts[currentItemSlotIndex].enabled = false;

        shotgunTexts[currentItemSlotIndex].text = string.Empty;
        shotgunTexts[currentItemSlotIndex].enabled = false;
    }

    private void HideAll(PlayerControllerB instance)
    {
        if (instance != GameNetworkManager.Instance.localPlayerController) return;

        for (var itemIndex = 0; itemIndex < totalItemSlots; itemIndex++)
        {
            Hide(itemIndex);
        }
    }

    private static Text CreateHudAndTextGameObject(string gameObjectName, int fontSize, Transform parent, Vector3 localPositionDelta)
    {
        return CreateHudAndTextGameObject(gameObjectName, fontSize, parent, TextAnchor.MiddleCenter, localPositionDelta);
    }

    private static Text CreateHudAndTextGameObject(string gameObjectName, int fontSize, Transform parent, TextAnchor anchor, Vector3 localPositionDelta)
    {
        var textGameObject = new GameObject(gameObjectName);
        var text = textGameObject.AddComponent<Text>();
        text.fontSize = fontSize;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontStyle = FontStyle.Normal;
        text.alignment = anchor;
        text.enabled = false;
        textGameObject.transform.SetParent(parent);
        textGameObject.transform.position = Vector3.zero;
        textGameObject.transform.localPosition = Vector3.zero + localPositionDelta;
        textGameObject.transform.localScale = Vector3.one;

        return text;
    }
}

