using PonyuDev.SherpaOnnx.Tts;
using UnityEngine;
using Zenject;

public class ScriptsInstaller : MonoInstaller
{
    [SerializeField] private EventHandler eventHandler;
    [SerializeField] private TtsOrchestrator ttsOrchestrator;
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
        Container.BindInterfacesAndSelfTo<TtsOrchestrator>()
            .FromInstance(ttsOrchestrator)
            .AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<TTSManager>()
            .FromNew()
            .AsSingle()
            .NonLazy();
    }
}