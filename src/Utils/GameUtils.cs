using UnityEngine;

namespace QualityCompany.Utils;

public static class GameUtils
{
    public static TimeOfDay TimeOfDay { get; private set; }
    public static StartOfRound StartOfRound { get; private set; }

    public static GameObject ShipGameObject { get; private set; }

    public static bool IsInShipMode => StartOfRound.inShipPhase;

    public static void Init()
    {
        TimeOfDay = TimeOfDay.Instance;
        StartOfRound = StartOfRound.Instance;
        ShipGameObject = GameObject.Find("Environment/HangarShip");
    }

    public static void Reset()
    {
        TimeOfDay = null;
        StartOfRound = null;
        ShipGameObject = null;
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

    public static bool IsLandedOnCompany()
    {
        if (StartOfRound is null) return false;

        return IsOnCompany() && !StartOfRound.inShipPhase;
    }
}
