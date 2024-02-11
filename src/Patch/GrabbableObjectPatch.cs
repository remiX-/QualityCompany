using HarmonyLib;
using QualityCompany.Service;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(GrabbableObject))]
internal class GrabbableObjectPatch
{
    private static readonly ACLogger _logger = new(nameof(GrabbableObject));

    [HarmonyPostfix]
    [HarmonyPatch("UseItemOnClient")]
    private static void UseItemOnClientPatch(GrabbableObject __instance)
    {
        OnItemActivate(__instance);
    }
}