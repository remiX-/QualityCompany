using GameNetcodeStuff;
using QualityCompany.Modules.Core;
using QualityCompany.Service;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Modules.Scrap;

[Module]
internal class AirhornModule : MonoBehaviour
{
    private static readonly ACLogger Logger = new(nameof(AirhornModule));

    private static readonly List<string> AudioClipNames = new() { "yippee-come-back.mp3", "test.mp3" };
    private static readonly AudioClip[] Clips = new AudioClip[AudioClipNames.Count];

    private static int _currentIndex;

    [ModuleOnLoad]
    private static AirhornModule Handle()
    {
        var go = new GameObject(nameof(AirhornModule));
        return go.AddComponent<AirhornModule>();
    }

    private void Awake()
    {
        for (var index = 0; index < AudioClipNames.Count; index++)
        {
            StartCoroutine(LoadAudioClip(index, Path.Combine(Plugin.Instance.PluginPath, AudioClipNames[index])));
        }

        PlayerGrabObjectClientRpc2 += OnPickup;
        ItemActivate += OnActivate;
    }

    private static void OnPickup(PlayerControllerB instance, bool isLocalPlayer, GrabbableObject grabbedObject)
    {
        if (grabbedObject is null) return;
        if (grabbedObject.itemProperties.name != "Airhorn") return;
        if (!grabbedObject.TryGetComponent<NoisemakerProp>(out var noisemakerProps)) return;

        Logger.LogMessage("We found an Airhorn exd");

        // noisemakerProps.noiseSFX[0] = Clips[_currentIndex];
        // noisemakerProps.noiseSFXFar[0] = Clips[_currentIndex];
        // _currentIndex++;
    }

    private static void OnActivate(GrabbableObject grabbedObject)
    {
        if (grabbedObject is null) return;
        if (grabbedObject.itemProperties.name != "Airhorn") return;
        if (!grabbedObject.TryGetComponent<NoisemakerProp>(out var noisemakerProps)) return;

        noisemakerProps.noiseSFX[0] = Clips[_currentIndex];
        noisemakerProps.noiseSFXFar[0] = Clips[_currentIndex];

        _currentIndex++;
        if (_currentIndex >= Clips.Length) _currentIndex = 0;

        Logger.LogMessage($"{grabbedObject.playerHeldBy.playerUsername} activated an Airhorn. Changed to to {_currentIndex}");
    }

    private static IEnumerator LoadAudioClip(int index, string filePath)
    {
        var audioClip = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.MPEG);
        yield return audioClip.SendWebRequest();

        if (audioClip.error is not null)
        {
            Logger.LogError("Error loading sounds: " + filePath + "\n" + audioClip.error);
            yield break;
        }

        var content = DownloadHandlerAudioClip.GetContent(audioClip);
        if (content is null || content.loadState != AudioDataLoadState.Loaded)
        {
            Logger.LogError($"Load failed for audio: {filePath}");
            yield break;
        }

        Logger.LogInfo($"Loaded {filePath} @ index {index}");
        content.name = Path.GetFileName(filePath);
        Clips[index] = content;
    }
}
