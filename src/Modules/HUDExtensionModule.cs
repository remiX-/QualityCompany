using AdvancedCompany.Service;
using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AdvancedCompany.Service.GameEvents;

namespace AdvancedCompany.Modules;

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

    // private Text _text;
    private readonly List<Text> scrapTexts = new();
    private readonly List<Text> shotgunTexts = new();

    // Maybe some kind of [ModuleOnSpawn] attribute?
    public static void Spawn()
    {
        var scrapUI = new GameObject(nameof(HUDExtensionModule));
        scrapUI.AddComponent<HUDExtensionModule>();
    }

    private void Awake()
    {
        _logger.LogMessage("Awake");
        Instance = this;

        transform.SetParent(HUDManager.Instance.HUDContainer.transform);
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        var scrapLocalPositionDelta = new Vector3(-6 * 2, 0f);

        for (var i = 0; i < HUDManager.Instance.itemSlotIconFrames.Length; i++)
        {
            var text = CreateHudAndTextGameObject($"HUDScrapUI{i}", 6, HUDManager.Instance.itemSlotIconFrames[i].gameObject.transform, scrapLocalPositionDelta);
            var shotgunText = CreateHudAndTextGameObject($"HUDShotgunAmmoUI{i}", 16, HUDManager.Instance.itemSlotIconFrames[i].gameObject.transform, Vector3.zero);

            scrapTexts.Add(text);
            shotgunTexts.Add(shotgunText);
        }

        Attach();
    }

    // Maybe some kind of [ModuleOnAttach] attribute?
    private void Attach()
    {
        PlayerGrabObjectClientRpc += UpdateUI;
        PlayerThrowObjectClientRpc += UpdateUI;
        PlayerShotgunShoot += UpdateUI;
        PlayerShotgunReload += UpdateUI;
        Disconnected += Detach;
    }

    // Maybe some kind of [ModuleOnDetach] attribute?
    private void Detach(GameNetworkManager instance)
    {
        PlayerGrabObjectClientRpc -= UpdateUI;
        PlayerThrowObjectClientRpc -= UpdateUI;
        PlayerShotgunShoot -= UpdateUI;
        PlayerShotgunReload -= UpdateUI;
        Disconnected -= Detach;
    }

    private void UpdateUI(PlayerControllerB instance)
    {
        _logger.LogMessage($"UpdateUI: _text:");
        if (instance != GameNetworkManager.Instance.localPlayerController) return;

        _logger.LogMessage($"UpdateUI: currentItemSlot: {instance.currentItemSlot} | currentlyHeldObjectServer: {instance.currentlyHeldObjectServer?.name ?? "null"}");

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

        //▲
        text.enabled = true;
        text.text = "\u258d" + currentHeldItem.scrapValue;
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

    private static Text CreateHudAndTextGameObject(string gameObjectName, int fontSize, Transform parent, Vector3 localPositionDelta)
    {
        var textGameObject = new GameObject(gameObjectName);
        var text = textGameObject.AddComponent<Text>();
        text.fontSize = fontSize;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontStyle = FontStyle.Normal;
        text.alignment = TextAnchor.MiddleCenter;
        text.enabled = false;
        textGameObject.transform.SetParent(parent);
        textGameObject.transform.position = Vector3.zero;
        textGameObject.transform.localPosition = Vector3.zero + localPositionDelta; //new Vector3(-text.fontSize * 2, 0f);
        textGameObject.transform.localScale = Vector3.one;

        return text;
    }
}

