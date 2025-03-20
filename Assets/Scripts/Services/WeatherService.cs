using System;
using UnityEngine;
using Zenject;

public class WeatherService
{
    private readonly RequestQueue requestQueue;
    private readonly string weatherUrl = "https://api.weather.gov/gridpoints/TOP/32,81/forecast";
    private const string WEATHER_REQUEST_TYPE = "weather";
    
    [Inject]
    public WeatherService(RequestQueue requestQueue)
    {
        this.requestQueue = requestQueue;
    }
    
    // Получить данные о погоде
    public void GetWeather(Action<WeatherData> onSuccess, Action<string> onError)
    {
        requestQueue.AddRequest(
            weatherUrl,
            jsonResponse => {
                try {
                    // Создаем обертку для корректного парсинга JSON
                    string wrappedJson = "{\"result\":" + jsonResponse + "}";
                    WeatherDataWrapper wrapper = JsonUtility.FromJson<WeatherDataWrapper>(wrappedJson);
                    onSuccess?.Invoke(wrapper.result);
                }
                catch (Exception e) {
                    Debug.LogError($"Error parsing weather data: {e.Message}\nJSON: {jsonResponse.Substring(0, Mathf.Min(jsonResponse.Length, 100))}...");
                    onError?.Invoke($"Ошибка парсинга данных погоды: {e.Message}");
                }
            },
            errorMessage => onError?.Invoke(errorMessage),
            WEATHER_REQUEST_TYPE
        );
    }
    
    // Отменить запросы погоды
    public void CancelWeatherRequests()
    {
        requestQueue.CancelRequests(WEATHER_REQUEST_TYPE);
    }
    
    // Обертка для данных о погоде
    [Serializable]
    private class WeatherDataWrapper
    {
        public WeatherData result;
    }
    
    // Класс для данных о погоде
    [Serializable]
    public class WeatherData
    {
        public Properties properties;
        
        [Serializable]
        public class Properties
        {
            public Period[] periods;
        }
        
        [Serializable]
        public class Period
        {
            public string name;
            public int temperature;
            public string temperatureUnit;
            public string icon;
        }
    }
}