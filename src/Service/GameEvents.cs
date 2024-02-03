using System;
using AdvancedCompany.Utils;
using GameNetcodeStuff;

namespace AdvancedCompany.Service;

public class GameEvents
{
    public static event HUDEvent HudManagerStart;
    public delegate void HUDEvent(HUDManager instance);

    public static event PlayerControllerEvent PlayerGrabObjectClientRpc;
    public static event PlayerControllerEvent PlayerSwitchToItemSlot;
    public static event PlayerControllerEvent PlayerDiscardHeldObject;
    public static event PlayerControllerEvent PlayerShotgunShoot;
    public static event PlayerControllerEvent PlayerShotgunReload;
    public delegate void PlayerControllerEvent(PlayerControllerB instance);


    public static void OnHudManagerStart(HUDManager instance)
    {
        Logger.LogDebug("[GameEvents] OnHudManagerStart");
        HudManagerStart?.Invoke(instance);
    }

    public static void OnPlayerGrabObjectClientRpc(PlayerControllerB instance)
    {
        Logger.LogDebug("[GameEvents] OnPlayerBeginGrabObject");
        PlayerGrabObjectClientRpc?.Invoke(instance);
    }

    public static void OnPlayerSwitchToItemSlot(PlayerControllerB instance)
    {
        Logger.LogDebug("[GameEvents] OnPlayerSwitchToItemSlot");
        PlayerSwitchToItemSlot?.Invoke(instance);
    }

    public static void OnPlayerDiscardHeldObject(PlayerControllerB instance)
    {
        Logger.LogDebug("[GameEvents] OnPlayerDiscardHeldObject");
        PlayerDiscardHeldObject?.Invoke(instance);
    }

    public static void OnPlayerShotgunShoot(PlayerControllerB instance)
    {
        Logger.LogDebug("[GameEvents] OnPlayerShootShotgun");
        PlayerShotgunShoot?.Invoke(instance);
    }

    public static void OnPlayerShotgunReload(PlayerControllerB instance)
    {
        Logger.LogDebug("[GameEvents] OnPlayerShotgunReload");
        PlayerShotgunReload?.Invoke(instance);
    }
}

