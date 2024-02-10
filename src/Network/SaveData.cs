using System;

namespace QualityCompany.Network;

[Serializable]
internal class SaveData
{
    public int TotalLootValue { get; set; }
    public int TotalDaysPlayedForCurrentQuota { get; set; }
    public int TargetForSelling { get; set; } = 1250;

    internal void ResetGameState()
    {
        TotalLootValue = 0;
        TotalDaysPlayedForCurrentQuota = 0;
    }
}
