using AdvancedCompany.Components;
using AdvancedCompany.Service;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using Unity.Netcode;
using UnityEngine;

namespace AdvancedCompany.Network;

public class CompanyNetworkHandler : NetworkBehaviour
{
    public static CompanyNetworkHandler Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(CompanyNetworkHandler));

    internal SaveData SaveData { get; private set; } = new();

    private bool retrievedCfg;
    private bool retrievedSave;

    private void Start()
    {
        _logger.LogDebug("Start called 123123");

        Instance = this;

        if (NetworkManager.IsHost)
        {
            var saveNum = GameNetworkManager.Instance.saveFileNum.ToString();
            var filePath = Path.Combine(Application.persistentDataPath, $"AdvancedCompany_{saveNum}.json");
            if (File.Exists(filePath))
            {
                _logger.LogInfo($"  > HOST: Loading save file for slot {saveNum}.");
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
    public void ServerSaveFileServerRpc()
    {
        var saveNum = GameNetworkManager.Instance.saveFileNum.ToString();
        var filePath = Path.Combine(Application.persistentDataPath, $"AdvancedCompany_{saveNum}.json");
        var json = JsonConvert.SerializeObject(SaveData);
        File.WriteAllText(filePath, json);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendConfigServerRpc()
    {
        _logger.LogInfo($"SendConfigServerRpc > from client");
        var json = JsonConvert.SerializeObject(Plugin.Instance.PluginConfig);
        SendConfigClientRpc(json);
    }

    [ClientRpc]
    private void SendConfigClientRpc(string json)
    {
        if (retrievedCfg)
        {
            _logger.LogInfo("Config has already been received from host on this client, disregarding.");
            return;
        }

        var cfg = JsonConvert.DeserializeObject<PluginConfig>(json);
        if (cfg != null && !IsHost && !IsServer)
        {
            _logger.LogInfo("Config received, deserializing and constructing...");
            Plugin.Instance.PluginConfig = cfg;
            retrievedCfg = true;
        }

        if (IsHost || IsServer)
        {
            StartCoroutine(WaitALittleToShareTheFile());
        }
    }

    private IEnumerator WaitALittleToShareTheFile()
    {
        yield return new WaitForSeconds(0.5f);
        _logger.LogInfo("Now sharing save file with clients...");
        ShareSaveServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShareSaveServerRpc()
    {
        var json = JsonConvert.SerializeObject(SaveData);
        ShareSaveClientRpc(json);
    }

    [ClientRpc]
    public void ShareSaveClientRpc(string json)
    {
        if (retrievedSave)
        {
            _logger.LogDebug("Save file already received from host, disregarding.");
            return;
        }

        retrievedSave = true;
        _logger.LogDebug("Save file received, registering.");

        SaveData = JsonConvert.DeserializeObject<SaveData>(json);
    }

    // void Update()
    // {
    //     _logger.LogDebug($"IsInShipMode: {GameUtils.IsInShipMode}");
    // }

    [ServerRpc(RequireOwnership = false)]
    public void SyncDepositDeskTotalValueServerRpc()
    {
        _logger.LogDebug("UpdateSellTargetServerRpc");

        SyncDepositDeskTotalValueClientRpc();
    }

    [ClientRpc]
    public void SyncDepositDeskTotalValueClientRpc()
    {
        _logger.LogDebug("SyncDepositDeskTotalValueClientRpc");

        OvertimeMonitor.UpdateMonitor();
    }

    public override void OnNetworkSpawn()
    {
        _logger.LogDebug("OnNetworkSpawn");

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            // _logger.LogDebug($"1...{Instance?.gameObject} | {Instance?.gameObject?.GetComponent<NetworkObject>()}");
            // Instance?.gameObject?.GetComponent<NetworkObject>()?.Despawn();
        }

        _logger.LogDebug("2");

        Instance = this;

        _logger.LogDebug("3");

        base.OnNetworkSpawn();

        _logger.LogDebug("4");
    }
}