using QualityCompany.Modules.Core;
using UnityEngine;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Modules.Inventory;

[Module(Delayed = true)]
internal class ShotgunAmmoModule : InventoryBaseUI
{
    private static readonly Color TEXT_COLOR_FULL = new(0f, 1f, 0f, 0.75f);
    private static readonly Color TEXT_COLOR_HALF = new(1f, 243f / 255f, 36f / 255f, 0.75f);
    private static readonly Color TEXT_COLOR_EMPTY = new(1f, 0f, 0f, 0.75f);

    public ShotgunAmmoModule() : base(nameof(ShotgunAmmoModule))
    { }

    [ModuleOnStart]
    private static ShotgunAmmoModule Spawn()
    {
        if (!Plugin.Instance.PluginConfig.InventoryShowShotgunAmmoCounterUI) return null;

        var scrapUI = new GameObject(nameof(ShotgunAmmoModule));
        return scrapUI.AddComponent<ShotgunAmmoModule>();
    }

    private new void Awake()
    {
        base.Awake();
        _logger.LogDebug("Module.Awake");

        totalItemSlots = HUDManager.Instance.itemSlotIconFrames.Length;

        for (var i = 0; i < totalItemSlots; i++)
        {
            CreateInventoryGameObject($"HUDShotgunAmmoUI{i}", 16, HUDManager.Instance.itemSlotIconFrames[i].gameObject.transform);
        }
    }

    [ModuleOnAttach]
    private void Attach()
    {
        _logger.LogDebug($"Attach {nameof(ShotgunAmmoModule)}");
        PlayerGrabObjectClientRpc += OnRpcUpdate;
        PlayerThrowObjectClientRpc += OnRpcUpdate;
        PlayerDiscardHeldObject += OnUpdate;
        PlayerDropAllHeldItems += HideAll;
        PlayerDeath += HideAll;
        PlayerShotgunShoot += OnUpdate;
        PlayerShotgunReload += OnUpdate;
    }

    [ModuleOnDetach]
    private void Detach()
    {
        _logger.LogDebug($"Detaching {nameof(ShotgunAmmoModule)}");
        PlayerGrabObjectClientRpc -= OnRpcUpdate;
        PlayerThrowObjectClientRpc -= OnRpcUpdate;
        PlayerDiscardHeldObject -= OnUpdate;
        PlayerDropAllHeldItems -= HideAll;
        PlayerDeath -= HideAll;
        PlayerShotgunShoot -= OnUpdate;
        PlayerShotgunReload -= OnUpdate;
    }

    protected override void OnUpdate(GrabbableObject item, int index)
    {
        var shotgunItem = item.GetComponent<ShotgunItem>();
        if (shotgunItem is null) return;

        var shellsLoaded = shotgunItem.shellsLoaded;
        var color = shellsLoaded switch
        {
            2 => TEXT_COLOR_FULL,
            1 => TEXT_COLOR_HALF,
            _ => TEXT_COLOR_EMPTY
        };

        UpdateItemSlotText(index, shellsLoaded.ToString(), color);
    }
}