using TMPro;
using UnityEngine;
using Zenject;

public class SystemInstaller : MonoInstaller
{
    [SerializeField] private TMP_Text inputField;
    [SerializeField] private TMP_Text outputField;
    public override void InstallBindings()
    {
        Container.Bind<TMP_Text>()
            .WithId("InputField")
            .FromInstance(inputField)
            .AsCached();
        Container.Bind<TMP_Text>()
            .WithId("OutputField")
            .FromInstance(outputField)
            .AsCached();
    }
}