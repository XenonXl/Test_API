using UnityEngine;
using Zenject;
public class UIInstaller : MonoInstaller
{
    [Header("UI компоненты")]
    [SerializeField] private PopupController popupControllerPrefab; // Изменено на префаб
    
    [Header("Контроллеры и вкладки")]
    [SerializeField] private TabController tabController;
    [SerializeField] private WeatherTab weatherTab;
    [SerializeField] private DogBreedsTab dogBreedsTab;
    [SerializeField] private BreedItem breedItemPrefab;
    public override void InstallBindings()
    {
        InstallPopupController();
        InstallTabs();
        InstallBreedItems();
    }
    private void InstallPopupController()
    {
        // Регистрируем PopupController из префаба
        Container.Bind<PopupController>()
            .FromComponentInNewPrefab(popupControllerPrefab)
            .UnderTransform(transform) // или другой подходящий transform
            .AsSingle()
            .NonLazy();
    }
    private void InstallTabs()
    {
        Container.Bind<TabController>()
            .FromInstance(tabController)
            .AsSingle();
        Container.Bind<WeatherTab>()
            .FromInstance(weatherTab)
            .AsSingle();
            
        Container.Bind<DogBreedsTab>()
            .FromInstance(dogBreedsTab)
            .AsSingle();
    }
    private void InstallBreedItems()
    {
        if (breedItemPrefab != null)
        {
            Container.BindFactory<Transform, BreedItem, BreedItem.Factory>()
                .FromComponentInNewPrefab(breedItemPrefab);
        }
    }
    private void ValidateComponents()
    {
        if (popupControllerPrefab == null)
            Debug.LogError("UIInstaller: popupControllerPrefab не назначен!");
        if (tabController == null)
            Debug.LogError("UIInstaller: tabController не назначен!");
        if (weatherTab == null)
            Debug.LogError("UIInstaller: weatherTab не назначен!");
        if (dogBreedsTab == null)
            Debug.LogError("UIInstaller: dogBreedsTab не назначен!");
        if (breedItemPrefab == null)
            Debug.LogError("UIInstaller: breedItemPrefab не назначен!");
    }
}