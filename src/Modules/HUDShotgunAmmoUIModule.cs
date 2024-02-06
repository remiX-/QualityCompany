using GameNetcodeStuff;
using QualityCompany.Service;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Modules;

internal class HUDShotgunAmmoUIModule : MonoBehaviour
{
    private readonly ACLogger _logger = new(nameof(HUDShotgunAmmoUIModule));

    private static readonly Color TEXT_COLOR_FULL = new(0f, 1f, 0f, 0.75f);
    private static readonly Color TEXT_COLOR_HALF = new(1f, 243f / 255f, 36f / 255f, 0.75f);
    private static readonly Color TEXT_COLOR_EMPTY = new(1f, 0f, 0f, 0.75f);

    private readonly List<TextMeshProUGUI> texts = new();

    private int totalItemSlots = 4; // game default

    // Maybe some kind of [ModuleOnSpawn] attribute?
    public static void Spawn()
    {
        if (!Plugin.Instance.PluginConfig.InventoryShowShotgunAmmoCounterUI) return;

        var scrapUI = new GameObject(nameof(HUDShotgunAmmoUIModule));
        scrapUI.AddComponent<HUDShotgunAmmoUIModule>();
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
            var shotgunText = CreateHudAndTextGameObject($"HUDShotgunAmmoUI{i}", 16, HUDManager.Instance.itemSlotIconFrames[i].gameObject.transform);

            texts.Add(shotgunText);
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

        // Show shotgun ammo counter UI if currently held item has a ShotgunItem component
        var shotgunItem = instance.currentlyHeldObjectServer.GetComponent<ShotgunItem>();
        if (shotgunItem is not null)
        {
            ShowShotgunAmmoText(instance.currentItemSlot, shotgunItem.shellsLoaded);
        }
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

            var shotgunItem = instance.ItemSlots[i].GetComponent<ShotgunItem>();
            if (shotgunItem is not null)
            {
                ShowShotgunAmmoText(i, shotgunItem.shellsLoaded);
            }
        }
    }

    private void ShowShotgunAmmoText(int currentItemSlotIndex, int shellsLoaded)
    {
        var text = texts[currentItemSlotIndex];
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

    private TextMeshProUGUI CreateHudAndTextGameObject(string gameObjectName, int fontSize, Transform parent)
    {
        var hangerShipHeaderText = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText");
        var textObject = Instantiate(hangerShipHeaderText, parent);
        textObject.name = gameObjectName;
        textObject.transform.position = Vector3.zero;
        textObject.transform.localPosition = Vector3.zero;
        textObject.transform.localScale = Vector3.one;
        textObject.transform.rotation = Quaternion.Euler(Vector3.zero);
        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.enabled = false;

        return text;
    }
}

