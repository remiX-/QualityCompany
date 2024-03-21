using GameNetcodeStuff;
using QualityCompany.Service;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace QualityCompany.Modules.Inventory;

internal abstract class InventoryBaseUI : MonoBehaviour
{
    protected readonly ModLogger Logger;

    protected readonly List<TextMeshProUGUI> Texts = new();

    private GameObject _baseTextToCopyGameObject;

    protected InventoryBaseUI(string moduleName)
    {
        Logger = new ModLogger(moduleName);
    }

    #region Lifecycle
    protected void Awake()
    {
        _baseTextToCopyGameObject = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText");

        transform.SetParent(HUDManager.Instance.HUDContainer.transform);
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
    }

    // protected void Start()
    // {
    //     Destroy(gameObject);
    // }
    #endregion

    #region UI Updates
    protected void OnUpdate(PlayerControllerB instance)
    {
        if (!instance.IsOwner) return;

        if (Plugin.Instance.PluginConfig.InventoryForceUpdateAllSlotsOnDiscard)
        {
            ForceUpdateAllSlots(instance);
            return;
        }

        if (instance.currentlyHeldObjectServer is null)
        {
            Hide(instance.currentItemSlot);
            return;
        }

        OnUpdate(instance.currentlyHeldObjectServer, instance.currentItemSlot);
    }

    protected abstract void OnUpdate(GrabbableObject go, int index);
    #endregion

    #region UI
    protected TextMeshProUGUI CreateInventoryGameObject(string gameObjectName, int fontSize, Transform parent, Vector3? localPositionDelta = null)
    {
        var textObject = Instantiate(_baseTextToCopyGameObject, parent);
        textObject.name = gameObjectName;
        textObject.transform.position = Vector3.zero;
        textObject.transform.localPosition = localPositionDelta ?? Vector3.zero;
        textObject.transform.localScale = Vector3.one;
        textObject.transform.rotation = Quaternion.Euler(Vector3.zero);
        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.enabled = false;

        return text;
    }

    protected void ForceUpdateAllSlots(PlayerControllerB instance)
    {
        for (var i = 0; i < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; i++)
        {
            if (instance.ItemSlots[i] is null)
            {
                Hide(i);
                continue;
            }

            OnUpdate(instance.ItemSlots[i], i);
        }
    }

    protected virtual void UpdateItemSlotText(int index, string text, Color color)
    {
        var textComponent = Texts[index];
        textComponent.enabled = true;
        textComponent.text = text;
        textComponent.color = color;
    }

    protected virtual void Hide(int currentItemSlotIndex)
    {
        Texts[currentItemSlotIndex].text = string.Empty;
        Texts[currentItemSlotIndex].enabled = false;
    }

    protected void HideAll(PlayerControllerB instance)
    {
        if (!instance.IsOwner) return;

        for (var itemIndex = 0; itemIndex < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; itemIndex++)
        {
            Hide(itemIndex);
        }
    }
    #endregion
}