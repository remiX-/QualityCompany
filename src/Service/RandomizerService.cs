using System;

namespace QualityCompany.Service;

internal class RandomizerService
{
    private readonly Random _random = new();

    public RandomizerService()
    {
        Plugin.Instance.Log.LogMessage("RandomizerService.Start");
    }

    public int GetInt(int min, int max)
    {
        return _random.Next(min, max);
    }
}
