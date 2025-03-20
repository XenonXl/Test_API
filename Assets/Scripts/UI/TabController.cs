// TabController.cs - Контроллер для переключения вкладок
using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class TabController : MonoBehaviour
{
    [SerializeField] private Button weatherTabButton;
    [SerializeField] private Button dogBreedsTabButton;
    
    private WeatherTab weatherTab;
    private DogBreedsTab dogBreedsTab;
    
    [Inject]
    public void Construct(WeatherTab weatherTab, DogBreedsTab dogBreedsTab)
    {
        this.weatherTab = weatherTab;
        this.dogBreedsTab = dogBreedsTab;
        
        // Подписываемся на нажатия кнопок
        weatherTabButton.onClick.AddListener(ShowWeatherTab);
        dogBreedsTabButton.onClick.AddListener(ShowDogBreedsTab);
        
        // По умолчанию показываем вкладку погоды
        ShowWeatherTab();
    }
    
    public void ShowWeatherTab()
    {
        // Скрываем вкладку пород и показываем вкладку погоды
        dogBreedsTab.Hide();
        weatherTab.Show();
    }
    
    public void ShowDogBreedsTab()
    {
        // Скрываем вкладку погоды и показываем вкладку пород
        weatherTab.Hide();
        dogBreedsTab.Show();
    }
}