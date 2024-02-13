using GameNetcodeStuff;
using QualityCompany.Service;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace QualityCompany.Modules.Inventory;

internal abstract class InventoryBaseUI : MonoBehaviour
{
    protected readonly ACLogger _logger;

    protected readonly List<TextMeshProUGUI> texts = new();

    private GameObject baseTextToCopyGameObject;

    protected InventoryBaseUI(string moduleName)
    {
        _logger = new ACLogger(moduleName);
    }

    #region Lifecycle
    protected void Awake()
    {
        baseTextToCopyGameObject = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText");

        transform.SetParent(HUDManager.Instance.HUDContainer.transform);
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
    }

    protected void Start()
    {
        Destroy(gameObject);
    }
    #endregion

    #region UI Updates
    protected void OnRpcUpdate(PlayerControllerB instance, bool isLocalPlayer)
    {
        if (!isLocalPlayer) return;

        OnUpdate(instance);
    }

    protected void OnUpdate(PlayerControllerB instance)
    {
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
        var textObject = Instantiate(baseTextToCopyGameObject, parent);
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
        var textComponent = texts[index];
        textComponent.enabled = true;
        textComponent.text = text;
        textComponent.color = color;
    }

    protected virtual void Hide(int currentItemSlotIndex)
    {
        texts[currentItemSlotIndex].text = string.Empty;
        texts[currentItemSlotIndex].enabled = false;
    }

    protected void HideAll(PlayerControllerB _, bool isLocalPlayer)
    {
        if (!isLocalPlayer) return;

        for (var itemIndex = 0; itemIndex < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; itemIndex++)
        {
            Hide(itemIndex);
        }
    }

    protected void OnPlayerDeath(PlayerControllerB instance)
    {
        if (instance != GameNetworkManager.Instance.localPlayerController) return;

        HideAll(instance, true);
    }
    #endregion
}