using System;
using AdvancedCompany.Utils;
using GameNetcodeStuff;

namespace AdvancedCompany.Service;

public class GameEvents
{
    public static event HUDEvent HudManagerStart;
    public delegate void HUDEvent(HUDManager instance);

    public static event PlayerControllerEvent PlayerBeginGrabObject;
    public static event PlayerControllerEvent PlayerSwitchToItemSlot;
    public delegate void PlayerControllerEvent(PlayerControllerB instance);


    public static void OnHudManagerStart(HUDManager instance)
    {
        Logger.LogDebug("[GameEvents] OnHudManagerStart");
        HudManagerStart?.Invoke(instance);
    }

    public static void OnPlayerBeginGrabObject(PlayerControllerB instance)
    {
        Logger.LogDebug("[GameEvents] OnPlayerBeginGrabObject");
        PlayerBeginGrabObject?.Invoke(instance);
    }

    public static void OnPlayerSwitchToItemSlot(PlayerControllerB instance)
    {
        Logger.LogDebug("[GameEvents] OnPlayerSwitchToItemSlot");
        PlayerSwitchToItemSlot?.Invoke(instance);
    }
}

