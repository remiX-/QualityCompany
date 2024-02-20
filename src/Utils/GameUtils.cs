using UnityEngine;

namespace QualityCompany.Utils;

/// <summary>
/// A utility class containing various game instance objects
/// </summary>
public static class GameUtils
{
    /// <summary>
    /// The games' TimeOfDay instance
    /// </summary>
    public static TimeOfDay TimeOfDay { get; private set; }

    /// <summary>
    /// The games' StartOfRound instance
    /// </summary>
    public static StartOfRound StartOfRound { get; private set; }

    /// <summary>
    /// The games' Terminal instance
    /// </summary>
    public static Terminal Terminal { get; internal set; }

    /// <summary>
    /// The games' HangarShip GameObject
    /// </summary>
    public static GameObject ShipGameObject { get; private set; }

    internal static void Init()
    {
        TimeOfDay = TimeOfDay.Instance;
        StartOfRound = StartOfRound.Instance;
        ShipGameObject = GameObject.Find("Environment/HangarShip");
    }

    internal static void Reset()
    {
        TimeOfDay = null;
        StartOfRound = null;
        Terminal = null;
        ShipGameObject = null;
    }

    public static bool IsCompanyBuyingAtFullRate()
    {
        return TimeOfDay.daysUntilDeadline == 0;
    }

    public static string CurrentLevel()
    {
        if (TimeOfDay is null) return "";

        return TimeOfDay.currentLevel.name;
    }

    public static string CurrentPlanet()
    {
        if (TimeOfDay is null) return "";

        return TimeOfDay.currentLevel?.PlanetName;
    }

    public static bool IsOnCompany()
    {
        return CurrentPlanet() == "71 Gordion";
    }

    public static bool IsLanded()
    {
        return StartOfRound.shipHasLanded;
    }

    public static bool IsOrbiting()
    {
        return !StartOfRound.inShipPhase;
    }

    public static bool IsLandedOnCompany()
    {
        if (StartOfRound is null) return false;

        return IsOnCompany() && IsOrbiting();
    }
}
