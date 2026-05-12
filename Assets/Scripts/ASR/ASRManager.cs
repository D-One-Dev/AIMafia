using UnityEngine;
using PonyuDev.SherpaOnnx.Asr.Offline;
using PonyuDev.SherpaOnnx.Asr.Offline.Engine;
using Zenject;
using System;

public class ASRManager : IInitializable, IDisposable
{
    private EventHandler _eventHandler;
    private AsrOrchestrator _orchestrator;

    private AudioClip _recordedClip;

    private string _microphoneDevice;
    private bool _isRecording = false;

    [Inject]
    public void Construct(AsrOrchestrator orchestrator, EventHandler eventHandler)
    {
        _orchestrator = orchestrator;

        _eventHandler = eventHandler;
        _eventHandler.OnRecognizePhrase += Recognize;
        _eventHandler.OnStartRecordingPhrase += StartRecording;
        _eventHandler.OnEndRecordingPhrase += StopRecording;
    }

    public void Initialize()
    {
        // Use default microphone
        if (Microphone.devices.Length > 0)
        {
            _microphoneDevice = Microphone.devices[0];
            Debug.Log("Using microphone: " + _microphoneDevice);
        }
        else
        {
            Debug.LogError("No microphone found!");
        }
    }

    private AudioClip TrimSilence(AudioClip clip, int position)
    {
        float[] samples = new float[position * clip.channels];
        clip.GetData(samples, 0);

        AudioClip newClip = AudioClip.Create(
            clip.name,
            position,
            clip.channels,
            clip.frequency,
            false
        );

        newClip.SetData(samples, 0);

        return newClip;
    }

    public void StartRecording()
    {
        if (_isRecording)
            return;

        // Parameters:
        // deviceName, loop, lengthSec, frequency
        _recordedClip = Microphone.Start(
            _microphoneDevice,
            false,
            10,
            16000
        );

        _isRecording = true;

        Debug.Log("Recording started");
    }

    public void StopRecording()
    {
        if (!_isRecording)
            return;

        int position = Microphone.GetPosition(_microphoneDevice);
        Microphone.End(_microphoneDevice);
        _recordedClip = TrimSilence(_recordedClip, position);

        _isRecording = false;

        Debug.Log("Recording stopped");

        Recognize(_recordedClip);

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