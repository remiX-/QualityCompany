using Newtonsoft.Json;
using QualityCompany.Modules.Ship;
using QualityCompany.Service;
using System.Collections;
using System.IO;
using Unity.Netcode;
using UnityEngine;

namespace QualityCompany.Network;

internal class CompanyNetworkHandler : NetworkBehaviour
{
    public static CompanyNetworkHandler Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(CompanyNetworkHandler));

    internal SaveData SaveData { get; private set; } = new();

    private bool retrievedCfg;
    private bool retrievedSave;

    private void Start()
    {
        Instance = this;

        if (NetworkManager.IsHost)
        {
            var saveNum = GameNetworkManager.Instance.saveFileNum.ToString();
            var filePath = Path.Combine(Application.persistentDataPath, $"QualityCompany_{saveNum}.json");
            if (File.Exists(filePath))
            {
                _logger.LogDebug($"  > HOST: Loading save file for slot {saveNum}.");
                var json = File.ReadAllText(filePath);
                SaveData = JsonConvert.DeserializeObject<SaveData>(json);
            }
            else
            {
                _logger.LogDebug($"  > HOST: No save file found for slot {saveNum}. Creating new.");
                SaveData = new SaveData();
            }
        }
        else
        {
            _logger.LogDebug("  > CLIENT: Requesting hosts config...");
            SendConfigServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = true)]
    internal void ServerSaveFileServerRpc()
    {
        var saveNum = GameNetworkManager.Instance.saveFileNum.ToString();
        var filePath = Path.Combine(Application.persistentDataPath, $"{PluginMetadata.PLUGIN_NAME}_{saveNum}.json");
        var json = JsonConvert.SerializeObject(SaveData);
        File.WriteAllText(filePath, json);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SendConfigServerRpc()
    {
        _logger.LogDebug("SendConfigServerRpc > from client");
        var json = JsonConvert.SerializeObject(Plugin.Instance.PluginConfig);
        SendConfigClientRpc(json);
    }

    [ClientRpc]
    private void SendConfigClientRpc(string json)
    {
        if (retrievedCfg)
        {
            _logger.LogDebug("Config has already been received from host on this client, disregarding.");
            return;
        }

        var cfg = JsonConvert.DeserializeObject<PluginConfig>(json);
        if (cfg != null && !IsHost && !IsServer)
        {
            _logger.LogDebug("Config received, deserializing and constructing...");
            Plugin.Instance.PluginConfig.ApplyHostConfig(cfg);
            retrievedCfg = true;
        }

        if (IsHost || IsServer)
        {
            StartCoroutine(WaitAndGetSaveFile());
        }
    }

    private IEnumerator WaitAndGetSaveFile()
    {
        yield return new WaitForSeconds(0.5f);
        _logger.LogDebug("Now sharing save file with clients...");
        ShareSaveServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShareSaveServerRpc()
    {
        var json = JsonConvert.SerializeObject(SaveData);
        ShareSaveClientRpc(json);
    }

    [ClientRpc]
    private void ShareSaveClientRpc(string json)
    {
        if (retrievedSave)
        {
            _logger.LogDebug("Save file already received from host, disregarding.");
            return;
        }

        retrievedSave = true;

        if (!IsHost && !IsServer)
        {
            _logger.LogDebug("Save file received, registering.");

            SaveData = JsonConvert.DeserializeObject<SaveData>(json);

            OvertimeMonitor.UpdateMonitor();
        }

        _logger.LogDebug(JsonConvert.SerializeObject(SaveData));
        Plugin.Instance.PluginConfig.DebugPrintConfig(_logger);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SyncDepositDeskTotalValueServerRpc()
    {
        _logger.LogDebug("UpdateSellTargetServerRpc");

        SyncDepositDeskTotalValueClientRpc();
    }

    [ClientRpc]
    private void SyncDepositDeskTotalValueClientRpc()
    {
        _logger.LogDebug("SyncDepositDeskTotalValueClientRpc");

        OvertimeMonitor.UpdateMonitor();
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            // This is recommended in the docs, but it doesn't seem to work
            // https://lethal.wiki/dev/advanced/networking#preventing-duplication
            // Instance?.gameObject?.GetComponent<NetworkObject>()?.Despawn();
        }

        Instance = this;

        base.OnNetworkSpawn();
    }
}