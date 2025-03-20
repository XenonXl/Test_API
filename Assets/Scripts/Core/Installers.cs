using UnityEngine;
using Zenject;

public class Installers : MonoInstaller
{
    public override void InstallBindings()
    {
        // Регистрируем очередь запросов
        Container.Bind<RequestQueue>().FromNewComponentOnNewGameObject().AsSingle();
        
        // Регистрируем сервисы
        Container.Bind<WeatherService>().AsSingle();
        Container.Bind<DogBreedsService>().AsSingle();
        
        // Регистрируем контроллер всплывающих окон
       // Container.Bind<PopupController>().AsSingle();
    }
}