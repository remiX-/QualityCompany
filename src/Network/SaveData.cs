using System;

namespace AdvancedCompany.Network;

[Serializable]
internal class SaveData
{
    public int TotalLootValue { get; set; }
    public int TotalDaysPlayedForCurrentQuota { get; set; }
}
