using System;
using PonyuDev.SherpaOnnx.Tts;
using Zenject;

public class TTSManager: IDisposable
{
    private EventHandler _eventHandler;
    private TtsOrchestrator _orchestrator;

    [Inject]
    public void Construct(TtsOrchestrator orchestrator, EventHandler eventHandler)
    {
        _orchestrator = orchestrator;

        _eventHandler = eventHandler;
        _eventHandler.OnSayPhrase += SayPhrase;
    }


    private void SayPhrase(string phrase)
    {
        _orchestrator.Service.SwitchProfile(UnityEngine.Random.Range(0, 4));
        _orchestrator.GenerateAndPlay(phrase);
    }

    public void Dispose()
    {
        _eventHandler.OnSayPhrase -= SayPhrase;
    }
}