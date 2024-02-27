using QualityCompany.Modules.Core;
using UnityEngine;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Modules.Inventory;

[Module(Delayed = true)]
internal class ShotgunAmmoModule : InventoryBaseUI
{
    private static readonly Color TextColorFull = new(0f, 1f, 0f, 0.75f);
    private static readonly Color TextColorHalf = new(1f, 243f / 255f, 36f / 255f, 0.75f);
    private static readonly Color TextColorEmpty = new(1f, 0f, 0f, 0.75f);

    public ShotgunAmmoModule() : base(nameof(ShotgunAmmoModule))
    { }

    [ModuleOnLoad]
    private static ShotgunAmmoModule Spawn()
    {
        if (!Plugin.Instance.PluginConfig.InventoryShowShotgunAmmoCounterUI) return null;

        var go = new GameObject(nameof(ShotgunAmmoModule));
        return go.AddComponent<ShotgunAmmoModule>();
    }

    private new void Awake()
    {
        base.Awake();

        for (var i = 0; i < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; i++)
        {
            texts.Add(CreateInventoryGameObject($"qc_HUDShotgunAmmoUI{i}", 16, HUDManager.Instance.itemSlotIconFrames[i].gameObject.transform));
        }
    }

    [ModuleOnAttach]
    private void Attach()
    {
        Logger.LogDebug($"Attach {nameof(ShotgunAmmoModule)}");
        PlayerGrabObjectClientRpc += OnUpdate;
        PlayerThrowObjectClientRpc += OnUpdate;
        PlayerDiscardHeldObject += OnUpdate;
        PlayerDropAllHeldItems += HideAll;
        PlayerDeath += HideAll;
        PlayerShotgunShoot += OnUpdate;
        PlayerShotgunReload += OnUpdate;
    }

    protected override void OnUpdate(GrabbableObject item, int index)
    {
        var shotgunItem = item.GetComponent<ShotgunItem>();
        if (shotgunItem is null) return;

        var shellsLoaded = shotgunItem.shellsLoaded;
        var color = shellsLoaded switch
        {
            2 => TextColorFull,
            1 => TextColorHalf,
            _ => TextColorEmpty
        };

        UpdateItemSlotText(index, shellsLoaded.ToString(), color);
    }
}