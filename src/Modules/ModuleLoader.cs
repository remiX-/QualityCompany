using AdvancedCompany.Service;
using System.Collections;
using UnityEngine;

namespace AdvancedCompany.Modules;

internal class ModuleLoader : MonoBehaviour
{
    public static ModuleLoader Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(ModuleLoader));

    private void Start()
    {
        _logger.LogMessage("Start");
        Instance = this;

        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        StartCoroutine(LoadScrapValueUIModulesCoroutine());
    }

    private IEnumerator LoadScrapValueUIModulesCoroutine()
    {
        _logger.LogMessage("LoadScrapValueUIModulesCoroutine -> waiting...");
        yield return new WaitForSeconds(2.0f);
        _logger.LogMessage("LoadScrapValueUIModulesCoroutine -> done!");

        // scrap value item ui
        HUDExtensionModule.Spawn();

        // for (var i = 0; i < HUDManager.Instance.itemSlotIconFrames.Length; i++)
        // {
        //     shotgun ammo counter ui
        //     var shotty = new GameObject($"shotgunAmmoUI{i}");
        //     shotty.AddComponent<ShotgunUIModule>();
        //     var shottyModule = shotty.GetComponent<ShotgunUIModule>();
        //     shottyModule.FrameParent = HUDManager.Instance.itemSlotIconFrames[i].gameObject;
        //     shottyModule.ItemIndex = i;
        //
        //     var uiMod = scrapUI.GetComponent<ScrapValueUIModule>();
        //     uiMod.FrameParent = HUDManager.Instance.itemSlotIconFrames[i].gameObject;
        //     uiMod.ItemIndex = i;
        //     var scrapUI = new GameObject($"hudScrapUI{i}");
        //     scrapUI.AddComponent<ScrapValueUIModule>();
        //     var uiMod = scrapUI.GetComponent<ScrapValueUIModule>();
        //     uiMod.FrameParent = HUDManager.Instance.itemSlotIconFrames[i].gameObject;
        //     uiMod.ItemIndex = i;
        // }
    }
}

