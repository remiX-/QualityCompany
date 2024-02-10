using HarmonyLib;

namespace QualityCompany.Patch;

internal class MenuPatch
{
    [HarmonyPatch(typeof(InitializeGame), "Start")]
    [HarmonyPrefix]
    private static void Start_Initialize(InitializeGame __instance)
    {
#if DEBUG
        Plugin.Instance.ACLogger.LogWarning("InitializeGame.Start");
        __instance.runBootUpScreen = false;
#endif
    }

    [HarmonyPatch(typeof(PreInitSceneScript), nameof(SkipToFinalSetting))]
    [HarmonyPrefix]
    private static bool SkipToFinalSetting(PreInitSceneScript __instance)
    {
#if DEBUG
        Plugin.Instance.ACLogger.LogWarning("Automatically launching ONLINE mode");
        __instance.ChooseLaunchOption(true);
        __instance.launchSettingsPanelsContainer.SetActive(false);

        return false;
#else
        return true;
#endif
    }
}