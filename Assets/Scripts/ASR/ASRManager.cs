using UnityEngine;
using PonyuDev.SherpaOnnx.Asr.Offline;
using PonyuDev.SherpaOnnx.Asr.Offline.Engine;
using Zenject;
using System;

public class ASRManager: IDisposable
{
    private EventHandler _eventHandler;
    private AsrOrchestrator _orchestrator;

    [Inject]
    public void Construct(AsrOrchestrator orchestrator, EventHandler eventHandler)
    {
        _orchestrator = orchestrator;

        _eventHandler = eventHandler;
        _eventHandler.OnRecognizePhrase += Recognize;
    }

    private async void Recognize(AudioClip audioClip)
    {
        AsrResult asyncResult = await _orchestrator.RecognizeFromClipAsync(audioClip);
        Debug.Log($"Async: {asyncResult?.Text}");
    }

    public void Dispose()
    {
        _eventHandler.OnRecognizePhrase += Recognize;
    }
}