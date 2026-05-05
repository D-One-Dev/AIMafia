using UnityEngine;
using Zenject;

public class ScriptsInstaller : MonoInstaller
{
    [SerializeField] private EventHandler eventHandler;
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
    }
}