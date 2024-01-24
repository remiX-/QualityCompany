using UnityEngine;

namespace AdvancedCompany.Game;

public static class GameUtils
{
    public static TimeOfDay TimeOfDay => TimeOfDay.Instance;
    public static StartOfRound StartOfRound => StartOfRound.Instance;

    private static GameObject _shipGameObject;
    public static GameObject ShipGameObject => GameObject.Find("Environment/HangarShip");

    // public static void Init()
    // {
    //     TimeOfDay = TimeOfDay.Instance;
    //     StartOfRound = StartOfRound.Instance;
    // }

    public static string CurrentLevel()
    {
        return TimeOfDay.currentLevel.name;
    }

    public static string CurrentPlanet()
    {
        return TimeOfDay.currentLevel.PlanetName;
    }

    public static bool IsOnCompany()
    {
        return CurrentPlanet() == "71 Gordion";
    }

    public static bool IsLandedOnCompany()
    {
        return IsOnCompany() && !StartOfRound.inShipPhase;
    }
}
