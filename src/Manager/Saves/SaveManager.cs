using Newtonsoft.Json;
using QualityCompany.Service;
using System;
using System.IO;
using Unity.Netcode;
using UnityEngine;

namespace QualityCompany.Manager.Saves;

internal class SaveManager
{
    private static readonly ACLogger Logger = new(nameof(SaveManager));

    internal static GameSaveData SaveData { get; private set; }

    private static bool IsHost => NetworkManager.Singleton.IsHost;
    private static bool HasNetworking => Plugin.Instance.PluginConfig.NetworkingEnabled;

    private static string _saveFileName;
    private static string _saveFilePath;

    internal static void Load()
    {
        if (IsHost)
        {
            var saveNum = GameNetworkManager.Instance.saveFileNum;
            Logger.LogDebug($"HOST: using save data file in slot number {saveNum}");
            _saveFileName = $"{PluginMetadata.PLUGIN_NAME}_{saveNum}.json";
        }
        else if (!HasNetworking)
        {
            Logger.LogDebug("CLIENT: networking is disabling, using .local save data file");
            _saveFileName = $"{PluginMetadata.PLUGIN_NAME}.local.json";
        }
        else
        {
            return;
        }

        _saveFilePath = Path.Combine(Application.persistentDataPath, _saveFileName);

        if (File.Exists(_saveFilePath))
        {
            Logger.LogDebug($"Loading save file: {_saveFileName}");
            var json = File.ReadAllText(_saveFilePath);
            LoadSaveJson(json);
        }
        else
        {
            Logger.LogDebug($"No save file found: {_saveFileName}, creating new");
            SaveData = new GameSaveData();
            Save();
        }
    }

    internal static void Save()
    {
        // host = always save to qc_saveNum
        // client + networking = don't save
        // client + no networking = save to qc.local
        if (!IsHost && HasNetworking) return;

        Logger.LogDebug($"Saving save data to {_saveFileName}");
        var json = JsonConvert.SerializeObject(SaveData);
        File.WriteAllText(_saveFilePath, json);
    }

    internal static void ClientLoadFromString(string saveJson)
    {
        Logger.LogDebug("CLIENT: Save file received, updating.");
        LoadSaveJson(saveJson);
    }

    private static void LoadSaveJson(string saveJson)
    {
        var jsonSaveData = JsonConvert.DeserializeObject<SaveData>(saveJson);
        SaveData.TotalShipLootAtStartForCurrentQuota = jsonSaveData.TotalShipLootAtStartForCurrentQuota;
        SaveData.TotalDaysPlayedForCurrentQuota = jsonSaveData.TotalDaysPlayedForCurrentQuota;
        SaveData.TargetForSelling = jsonSaveData.TargetForSelling;
    }
}

[Serializable]
file class SaveData
{
    public int TotalShipLootAtStartForCurrentQuota { get; set; }
    public int TotalDaysPlayedForCurrentQuota { get; set; }
    public int TargetForSelling { get; set; } = 1250;
}