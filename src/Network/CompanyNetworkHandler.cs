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

    private readonly ModLogger Logger = new(nameof(CompanyNetworkHandler));

    private bool _retrievedPluginConfig;
    private bool _retrievedSaveFile;

    private void Start()
    {
        Instance = this;

        if (IsHost) return;

        Logger.LogDebug("CLIENT: Requesting hosts config...");
        RequestPluginConfigServerRpc();
        RequestSaveDataServerRpc();

        StartCoroutine(ClientSanityCheck());
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPluginConfigServerRpc()
    {
        Logger.LogDebug("HOST: A client is requesting plugin config");
        var json = JsonConvert.SerializeObject(Plugin.Instance.PluginConfig);
        SendPluginConfigClientRpc(json);
    }

    [ClientRpc]
    private void SendPluginConfigClientRpc(string json)
    {
        if (IsHost || IsServer) return;

        if (_retrievedPluginConfig)
        {
            Logger.LogDebug("CLIENT: Config has already been received from host, disregarding.");
            return;
        }
        _retrievedPluginConfig = true;

        var cfg = JsonConvert.DeserializeObject<PluginConfig>(json);
        if (cfg is null)
        {
            Logger.LogError($"CLIENT: failed to deserialize plugin config from host, disregarding. raw json: {json}");
            return;
        }

        Logger.LogDebug("Config received, deserializing and constructing...");
        Plugin.Instance.PluginConfig.ApplyHostConfig(cfg);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSaveDataServerRpc()
    {
        Logger.LogDebug("HOST: A client is requesting save data");
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

        InfoMonitor.UpdateMonitor();
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SyncDepositDeskTotalValueServerRpc()
    {
        Logger.LogDebug("UpdateSellTargetServerRpc");

        SyncDepositDeskTotalValueClientRpc();
    }

    [ClientRpc]
    private void SyncDepositDeskTotalValueClientRpc()
    {
        Logger.LogDebug("SyncDepositDeskTotalValueClientRpc");

        InfoMonitor.UpdateMonitor();
    }

    private IEnumerator ClientSanityCheck()
    {
        yield return new WaitForSeconds(5.0f);

        if (!_retrievedPluginConfig)
        {
            Logger.LogError("CLIENT: Still have not received plugin config from host, something is wrong!");
        }

        if (!_retrievedSaveFile)
        {
            Logger.LogError("CLIENT: Still have not received game save data from host, something is wrong!");
        }
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;

        base.OnNetworkSpawn();
    }
}