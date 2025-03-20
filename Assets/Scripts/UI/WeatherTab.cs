using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Zenject;
using TMPro;

public class WeatherTab : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private TMP_Text forecastText;
    [SerializeField] private RawImage weatherIcon;
    
    private WeatherService weatherService;
    private Coroutine updateCoroutine;
    private Coroutine loadIconCoroutine;
    
    [Inject]
    public void Construct(WeatherService weatherService)
    {
        this.weatherService = weatherService;
    }
    
    // Показать вкладку
    public void Show()
    {
        rootPanel.SetActive(true);
        StartWeatherUpdates();
    }
    
    // Скрыть вкладку
    public void Hide()
    {
        rootPanel.SetActive(false);
        StopWeatherUpdates();
        weatherService.CancelWeatherRequests();
        
        // Сбросить иконку
        if (weatherIcon != null)
        {
            weatherIcon.gameObject.SetActive(false);
            weatherIcon.texture = null;
        }
    }
    
    // Запустить обновление погоды
    private void StartWeatherUpdates()
    {
        StopWeatherUpdates(); // Сначала остановим предыдущее обновление
        
        updateCoroutine = StartCoroutine(UpdateWeatherPeriodically());
    }
    
    // Остановить обновление погоды
    private void StopWeatherUpdates()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
        
        if (loadIconCoroutine != null)
        {
            StopCoroutine(loadIconCoroutine);
            loadIconCoroutine = null;
        }
    }
    
    // Корутина для периодического обновления погоды
    private IEnumerator UpdateWeatherPeriodically()
    {
        // Сразу запрашиваем погоду
        RequestWeatherUpdate();
        
        // Затем обновляем каждые 5 секунд
        while (true)
        {
            yield return new WaitForSeconds(5f);
            RequestWeatherUpdate();
        }
    }
    
    // Запросить обновление погоды
    private void RequestWeatherUpdate()
    {
        weatherService.GetWeather(OnWeatherReceived, OnWeatherError);
    }
    
    // Обработка полученных данных о погоде
    private void OnWeatherReceived(WeatherService.WeatherData data)
    {
        if (data?.properties?.periods != null && data.properties.periods.Length > 0)
        {
            var today = data.properties.periods[0];
            
            if (forecastText != null)
            {
                forecastText.text = $"Today - {today.temperature}{today.temperatureUnit}";
            }
            
            // Загружаем иконку
            if (!string.IsNullOrEmpty(today.icon) && weatherIcon != null)
            {
                LoadWeatherIcon(today.icon);
            }
            else
            {
                if (weatherIcon != null)
                {
                    weatherIcon.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogWarning("WeatherTab: Получены некорректные данные о погоде");
            if (forecastText != null)
            {
                forecastText.text = "Данные о погоде недоступны";
            }
            if (weatherIcon != null)
            {
                weatherIcon.gameObject.SetActive(false);
            }
        }
    }
    
    // Загрузка иконки погоды
    private void LoadWeatherIcon(string iconUrl)
    {
        if (loadIconCoroutine != null)
        {
            StopCoroutine(loadIconCoroutine);
        }
        
        loadIconCoroutine = StartCoroutine(LoadIconCoroutine(iconUrl));
    }
    
    // Корутина загрузки иконки
    private IEnumerator LoadIconCoroutine(string iconUrl)
    {
        if (weatherIcon == null)
        {
            Debug.LogError("WeatherTab: weatherIcon компонент не найден");
            yield break;
        }
        
        
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(iconUrl))
        {
            yield return request.SendWebRequest();
            
            // Проверяем, активна ли вкладка
            if (!rootPanel.activeSelf)
            {
                Debug.Log("WeatherTab: Вкладка больше не активна, прерываем загрузку иконки");
                yield break;
            }
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                weatherIcon.texture = texture;
                weatherIcon.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"Не удалось загрузить иконку погоды: {request.error}");
                weatherIcon.gameObject.SetActive(false);
            }
        }
        
        loadIconCoroutine = null;
    }
    
    // Обработка ошибки получения погоды
    private void OnWeatherError(string error)
    {
        Debug.LogError($"Ошибка получения данных о погоде: {error}");
        
        if (forecastText != null)
        {
            forecastText.text = "Ошибка получения данных о погоде";
        }
        
        if (weatherIcon != null)
        {
            weatherIcon.gameObject.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        StopWeatherUpdates();
    }
}