using System;
using System.Threading.Tasks;
using PonyuDev.SherpaOnnx.Tts;
using PonyuDev.SherpaOnnx.Tts.Engine;
using Zenject;

public class TTSManager : IDisposable
{
    private EventHandler _eventHandler;
    private TtsOrchestrator _orchestrator;

    [Inject]
    public void Construct(TtsOrchestrator orchestrator, EventHandler eventHandler)
    {
        _orchestrator = orchestrator;

        _eventHandler = eventHandler;
        // _eventHandler.OnSayPhrase += SayPhrase;
    }


    public Task<TtsResult> SayPhrase(string phrase, int voiceID)
    {
        _orchestrator.Service.SwitchProfile(voiceID);
        return _orchestrator.GenerateAndPlayAsync(phrase);
    }

    public void Dispose()
    {
        // _eventHandler.OnSayPhrase -= SayPhrase;
    }
}