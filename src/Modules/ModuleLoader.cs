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

        // GameEvents.HudManagerStart += LoadScrapValueUIModules;
        // GameEvents.PlayerDiscardHeldObject += UpdateUI;

        StartCoroutine(LoadScrapValueUIModulesCoroutine());
    }

    // private void Update()
    // {
    //     _logger.LogDebug("Update");
    // }

    // private void LoadScrapValueUIModules(HUDManager instance)
    // {
    //     StartCoroutine(LoadScrapValueUIModulesCoroutine());
    //     GameEvents.HudManagerStart -= LoadScrapValueUIModules;
    // }

    private IEnumerator LoadScrapValueUIModulesCoroutine()
    {
        _logger.LogMessage("LoadScrapValueUIModulesCoroutine -> waiting...");
        yield return new WaitForSeconds(3.0f);
        _logger.LogMessage("LoadScrapValueUIModulesCoroutine -> done!");

        for (var i = 0; i < HUDManager.Instance.itemSlotIconFrames.Length; i++)
        {
            // shotgun ammo counter ui
            var shotty = new GameObject($"shotgunAmmoUI{i}");
            shotty.AddComponent<ShotgunUIModule>();
            var shottyModule = shotty.GetComponent<ShotgunUIModule>();
            shottyModule.FrameParent = HUDManager.Instance.itemSlotIconFrames[i].gameObject;
            shottyModule.MyItemSlotIShouldListenTo = i;

            // scrap value item ui
            var scrapUI = new GameObject($"hudScrapUI{i}");
            scrapUI.AddComponent<ScrapValueUIModule>();
            var uiMod = scrapUI.GetComponent<ScrapValueUIModule>();
            uiMod.FrameParent = HUDManager.Instance.itemSlotIconFrames[i].gameObject;
            uiMod.MyItemSlotIShouldListenTo = i;
        }
    }
}

