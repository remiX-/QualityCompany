using Newtonsoft.Json;
using QualityCompany.Manager.Saves;
using QualityCompany.Modules.Ship;
using QualityCompany.Service;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace QualityCompany.Network;

internal class CompanyNetworkHandler : NetworkBehaviour
{
    public static CompanyNetworkHandler Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(CompanyNetworkHandler));

    private bool _retrievedPluginConfig;
    private bool _retrievedSaveFile;

    private void Start()
    {
        Instance = this;

        if (IsHost) return;

        _logger.LogDebug("CLIENT: Requesting hosts config...");
        RequestPluginConfigServerRpc();
        RequestSaveDataServerRpc();

        StartCoroutine(ClientSanityCheck());
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPluginConfigServerRpc()
    {
        _logger.LogDebug("HOST: A client is requesting plugin config");
        var json = JsonConvert.SerializeObject(Plugin.Instance.PluginConfig);
        SendPluginConfigClientRpc(json);
    }

    [ClientRpc]
    private void SendPluginConfigClientRpc(string json)
    {
        if (IsHost || IsServer) return;

        if (_retrievedPluginConfig)
        {
            _logger.LogDebug("CLIENT: Config has already been received from host, disregarding.");
            return;
        }
        _retrievedPluginConfig = true;

        var cfg = JsonConvert.DeserializeObject<PluginConfig>(json);
        if (cfg is null)
        {
            _logger.LogError($"CLIENT: failed to deserialize plugin config from host, disregarding. raw json: {json}");
            return;
        }

        _logger.LogDebug("Config received, deserializing and constructing...");
        Plugin.Instance.PluginConfig.ApplyHostConfig(cfg);
    }

    // private IEnumerator WaitAndRequestSaveFile()
    // {
    //     yield return new WaitForSeconds(0.5f);
    //     _logger.LogDebug("Now getting save file from host");
    //     RequestSaveDataServerRpc();
    // }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSaveDataServerRpc()
    {
        _logger.LogDebug("HOST: A client is requesting save data");
        var json = JsonConvert.SerializeObject(SaveManager.SaveData);
        SendSaveDataClientRpc(json);
    }

    [ClientRpc]
    private void SendSaveDataClientRpc(string json)
    {
        if (IsHost || IsServer) return;
        if (_retrievedSaveFile) return;
        _retrievedSaveFile = true;

        SaveManager.ClientLoadFromString(json);

        OvertimeMonitor.UpdateMonitor();
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

    private IEnumerator ClientSanityCheck()
    {
        yield return new WaitForSeconds(5.0f);

        if (!_retrievedPluginConfig)
        {
            _logger.LogError("CLIENT: Still have not received plugin config from host, something is wrong!");
        }

        if (!_retrievedSaveFile)
        {
            _logger.LogError("CLIENT: Still have not received game save data from host, something is wrong!");
        }
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