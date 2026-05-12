using UnityEngine;
using PonyuDev.SherpaOnnx.Asr.Online;
using PonyuDev.SherpaOnnx.Asr.Online.Engine;
using PonyuDev.SherpaOnnx.Common.Audio;
using PonyuDev.SherpaOnnx.Common.Audio.Config;
using Zenject;
using System;

public class OnlineASRManager : IInitializable, IDisposable
{
    private OnlineAsrOrchestrator _orchestrator;
    private EventHandler _eventHandler;

    [Inject]
    public void Construct(OnlineAsrOrchestrator orchestrator, EventHandler eventHandler)
    {
        _orchestrator = orchestrator;
        _eventHandler = eventHandler;

        _eventHandler.OnStartRecordingPhrase += SetupMicrophone;
        _eventHandler.OnEndRecordingPhrase += StopRecording;
    }

    private MicrophoneSource _mic;

    public void Initialize()
    {
        // if (_orchestrator.IsInitialized)
        //     SetupMicrophone();
        // else
        //     _orchestrator.Initialized += SetupMicrophone;

        _orchestrator.PartialResultReady += OnPartial;
        _orchestrator.FinalResultReady += OnFinal;
        _orchestrator.EndpointDetected += OnEndpoint;
    }

    public async void SetupMicrophone()
    {
        var micSettings = await MicrophoneSettingsLoader.LoadAsync();
        _mic = new MicrophoneSource(micSettings);
        _mic.SilenceDetected += OnSilenceDetected;
        bool started = await _mic.StartRecordingAsync();

        if (started)
            _orchestrator.ConnectMicrophone(_mic);
    }

    public void StopRecording()
    {
        _orchestrator.DisconnectMicrophone();
        _mic?.StopRecording();
    }

    private void OnPartial(OnlineAsrResult result)
    {
        Debug.Log($"Partial: {result.Text}");
    }

    private void OnFinal(OnlineAsrResult result)
    {
        Debug.Log($"Final: {result.Text}");
    }

    private void OnEndpoint()
    {
        Debug.Log("Endpoint detected — stream reset.");
    }

    private void OnSilenceDetected(string diagnosis)
    {
        // Microphone returned silence on all available paths.
        // Stop recording and notify the user.
        _orchestrator.DisconnectMicrophone();
        _mic?.StopRecording();
        Debug.LogWarning(
            "Voice capture unavailable on this device. " +
            "Diag: " + diagnosis);
    }

    public void Dispose()
    {
        // _orchestrator.Initialized -= SetupMicrophone;
        _orchestrator.PartialResultReady -= OnPartial;
        _orchestrator.FinalResultReady -= OnFinal;
        _orchestrator.EndpointDetected -= OnEndpoint;

        _eventHandler.OnStartRecordingPhrase -= SetupMicrophone;
        _eventHandler.OnEndRecordingPhrase -= StopRecording;

        if (_mic != null)
            _mic.SilenceDetected -= OnSilenceDetected;

        _orchestrator.DisconnectMicrophone();
        _mic?.Dispose();
    }
}