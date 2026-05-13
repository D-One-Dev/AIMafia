using PonyuDev.SherpaOnnx.Asr.Offline;
using PonyuDev.SherpaOnnx.Tts;
using UnityEngine;
using Zenject;

public class ScriptsInstaller : MonoInstaller
{
    [SerializeField] private EventHandler eventHandler;
    [SerializeField] private TtsOrchestrator ttsOrchestrator;
    [SerializeField] private AsrOrchestrator asrOrchestrator;
    public override void InstallBindings()
    {
        Container.Bind<EventHandler>()
            .FromInstance(eventHandler)
            .AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<LLMInputHandler>()
            .FromNew()
            .AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<DatabaseManager>()
            .FromNew()
            .AsSingle()
            .NonLazy();
        Container.Bind<TtsOrchestrator>()
            .FromInstance(ttsOrchestrator)
            .AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<TTSManager>()
            .FromNew()
            .AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<AsrOrchestrator>()
            .FromInstance(asrOrchestrator)
            .AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<ASRManager>()
            .FromNew()
            .AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<GameMaster>()
            .FromNew()
            .AsSingle()
            .NonLazy();
    }
}