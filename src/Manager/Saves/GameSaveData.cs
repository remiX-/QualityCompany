namespace QualityCompany.Manager.Saves;

internal class GameSaveData
{
    public int TotalShipLootAtStartForCurrentQuota { get; set; }
    public int TotalDaysPlayedForCurrentQuota { get; set; }
    public int TargetForSelling { get; set; } = 1250;

    internal void ResetGameState()
    {
        TotalShipLootAtStartForCurrentQuota = 0;
        TotalDaysPlayedForCurrentQuota = 0;
    }
}