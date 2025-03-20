using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RequestQueue : MonoBehaviour
{
    private Queue<WebRequestItem> requestQueue = new Queue<WebRequestItem>();
    private WebRequestItem currentRequest;
    private bool isProcessing;
    private bool useLocalData = true; // Для тестирования с локальными данными
    private Coroutine currentRequestCoroutine;

    // Класс для хранения информации о запросе
    public class WebRequestItem
    {
        public UnityWebRequest Request { get; private set; }
        public Action<string> OnSuccess { get; private set; }
        public Action<string> OnError { get; private set; }
        public string RequestType { get; private set; }
        public bool IsCancelled { get; set; }
        
        public WebRequestItem(UnityWebRequest request, Action<string> onSuccess, Action<string> onError, string requestType)
        {
            Request = request;
            OnSuccess = onSuccess;
            OnError = onError;
            RequestType = requestType;
            IsCancelled = false;
        }
    }

    private void Start()
    {
        // Проверяем интернет-соединение при запуске
        StartCoroutine(CheckInternetConnection());
    }

    // Проверка интернет-соединения без использования try-catch с yield
    private IEnumerator CheckInternetConnection()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://yandex.ru");
        request.timeout = 5;
        
        yield return request.SendWebRequest();
        
        bool isConnected = request.result == UnityWebRequest.Result.Success;
        Debug.Log($"Проверка интернет-соединения: {(isConnected ? "Доступно" : "Недоступно")}");
        
        if (!isConnected)
        {
            Debug.LogWarning("Интернет недоступен. Будут использоваться локальные данные.");
            useLocalData = true;
        }
        else
        {
            useLocalData = false;
        }
        
        request.Dispose();
    }

    // Добавить запрос в очередь
    public void AddRequest(string url, Action<string> onSuccess, Action<string> onError, string requestType)
    {
        Debug.Log($"RequestQueue: Добавлен запрос типа {requestType} к {url}");
        
        // Если используем локальные данные, возвращаем заглушку
        if (useLocalData)
        {
            StartCoroutine(ReturnMockData(requestType, onSuccess, onError));
            return;
        }
        
        try
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            WebRequestItem item = new WebRequestItem(request, onSuccess, onError, requestType);
            
            requestQueue.Enqueue(item);
            
            if (!isProcessing)
            {
                ProcessNextRequest();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RequestQueue: Ошибка при создании запроса: {e.Message}");
            onError?.Invoke($"Ошибка при создании запроса: {e.Message}");
        }
    }

    // Имитация данных для локального тестирования
    private IEnumerator ReturnMockData(string requestType, Action<string> onSuccess, Action<string> onError)
    {
        // Имитируем задержку сети
        yield return new WaitForSeconds(1f);
        
        string mockData = "";
        
        switch (requestType)
        {
            case "weather":
                mockData = "{\"properties\":{\"periods\":[{\"name\":\"Today\",\"temperature\":72,\"temperatureUnit\":\"F\",\"icon\":\"https://api.weather.gov/icons/land/day/sct?size=medium\"}]}}";
                break;
            case "breeds_list":
                mockData = "{\"data\":[" +
                    "{\"id\":\"1\",\"attributes\":{\"name\":\"Labrador Retriever\",\"description\":\"Friendly dog\"}}," +
                    "{\"id\":\"2\",\"attributes\":{\"name\":\"German Shepherd\",\"description\":\"Loyal dog\"}}," +
                    "{\"id\":\"3\",\"attributes\":{\"name\":\"Golden Retriever\",\"description\":\"Family dog\"}}," +
                    "{\"id\":\"4\",\"attributes\":{\"name\":\"Bulldog\",\"description\":\"Strong dog\"}}," +
                    "{\"id\":\"5\",\"attributes\":{\"name\":\"Beagle\",\"description\":\"Hunting dog\"}}," +
                    "{\"id\":\"6\",\"attributes\":{\"name\":\"Poodle\",\"description\":\"Smart dog\"}}," +
                    "{\"id\":\"7\",\"attributes\":{\"name\":\"Rottweiler\",\"description\":\"Guard dog\"}}," +
                    "{\"id\":\"8\",\"attributes\":{\"name\":\"Yorkshire Terrier\",\"description\":\"Small dog\"}}," +
                    "{\"id\":\"9\",\"attributes\":{\"name\":\"Boxer\",\"description\":\"Energetic dog\"}}," +
                    "{\"id\":\"10\",\"attributes\":{\"name\":\"Dachshund\",\"description\":\"Long dog\"}}" +
                    "]}";
                break;
            case "breed_details":
                mockData = "{\"data\":{\"id\":\"1\",\"attributes\":{\"name\":\"Labrador Retriever\",\"description\":\"The Labrador Retriever is a strongly built, medium-sized, short-coupled, dog possessing a sound, athletic, well-balanced conformation that enables it to function as a retrieving gun dog; the substance and soundness to hunt waterfowl or upland game for long hours under difficult conditions.\"}}}";
                break;
            default:
                onError?.Invoke("Неизвестный тип запроса");
                yield break;
        }
        
        Debug.Log($"RequestQueue: Возвращаем локальные данные для {requestType}");
        onSuccess?.Invoke(mockData);
    }

    // Отменить все запросы определенного типа
    public void CancelRequests(string requestType)
    {
        Debug.Log($"RequestQueue: Отмена запросов типа {requestType}");
        
        // Если используем локальные данные, просто логируем
        if (useLocalData)
        {
            Debug.Log($"RequestQueue: Имитация отмены запросов типа {requestType} (локальные данные)");
            return;
        }
        
        // Отменяем текущий запрос, если он соответствует типу
        if (currentRequest != null && currentRequest.RequestType == requestType)
        {
            Debug.Log($"RequestQueue: Отмена текущего запроса типа {requestType}");
            currentRequest.IsCancelled = true;
            currentRequest.Request.Abort();
            
            if (currentRequestCoroutine != null)
            {
                StopCoroutine(currentRequestCoroutine);
                currentRequestCoroutine = null;
            }
            
            currentRequest = null;
            isProcessing = false;
            ProcessNextRequest();
        }
        
        // Удаляем запросы указанного типа из очереди
        var tempQueue = new Queue<WebRequestItem>();
        int removedCount = 0;
        
        while (requestQueue.Count > 0)
        {
            var item = requestQueue.Dequeue();
            if (item.RequestType != requestType)
            {
                tempQueue.Enqueue(item);
            }
            else
            {
                removedCount++;
                item.IsCancelled = true;
                item.Request.Dispose();
            }
        }
        
        requestQueue = tempQueue;
        
        if (removedCount > 0)
        {
            Debug.Log($"RequestQueue: Удалено {removedCount} запросов типа {requestType} из очереди");
        }
    }

    // Обработать следующий запрос в очереди
    private void ProcessNextRequest()
    {
        if (requestQueue.Count == 0)
        {
            isProcessing = false;
            return;
        }

        isProcessing = true;
        currentRequest = requestQueue.Dequeue();
        currentRequestCoroutine = StartCoroutine(ExecuteRequest(currentRequest));
    }

    // Выполнить запрос
    private IEnumerator ExecuteRequest(WebRequestItem item)
    {
        if (item.IsCancelled)
        {
            Debug.Log("RequestQueue: Запрос был отменен перед выполнением");
            currentRequest = null;
            ProcessNextRequest();
            yield break;
        }
        
        Debug.Log($"RequestQueue: Выполняем запрос: {item.Request.url}");
        
        // Устанавливаем таймаут
        item.Request.timeout = 10;
        
        // Отправляем запрос
        yield return item.Request.SendWebRequest();
        
        // Проверяем, не был ли запрос отменен во время выполнения
        if (item.IsCancelled)
        {
            Debug.Log("RequestQueue: Запрос был отменен во время выполнения");
            item.Request.Dispose();
            currentRequest = null;
            ProcessNextRequest();
            yield break;
        }
        
        Debug.Log($"RequestQueue: Запрос завершен: {item.Request.url}, Результат: {item.Request.result}");
        
        try
        {
            if (item.Request.result == UnityWebRequest.Result.Success)
            {
                string responseText = item.Request.downloadHandler.text;
                Debug.Log($"RequestQueue: Запрос успешен. Размер ответа: {responseText.Length} символов");
                
                if (responseText.Length > 0)
                {
                    Debug.Log($"RequestQueue: Первые 100 символов ответа: {responseText.Substring(0, Mathf.Min(100, responseText.Length))}");
                }
                else
                {
                    Debug.LogWarning("RequestQueue: Получен пустой ответ");
                }
                
                item.OnSuccess?.Invoke(responseText);
            }
            else
            {
                Debug.LogError($"RequestQueue: Ошибка запроса: {item.Request.error}");
                item.OnError?.Invoke(item.Request.error);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RequestQueue: Исключение при обработке ответа: {e.Message}\n{e.StackTrace}");
            item.OnError?.Invoke($"Исключение: {e.Message}");
        }
        finally
        {
            // Освобождаем ресурсы
            item.Request.Dispose();
            
            // Это будет выполнено в любом случае
            currentRequest = null;
            currentRequestCoroutine = null;
            ProcessNextRequest();
        }
    }
    
    // Метод для переключения режима использования локальных данных
    public void SetUseLocalData(bool useLocal)
    {
        useLocalData = useLocal;
        Debug.Log($"RequestQueue: Режим локальных данных {(useLocalData ? "включен" : "выключен")}");
    }
    
    // Метод для тестирования API
    public void TestAPI()
    {
        StartCoroutine(TestAPIs());
    }
    
    private IEnumerator TestAPIs()
    {
        Debug.Log("Тестирование API...");
        
        // Тест API погоды
        yield return TestAPIEndpoint("https://api.weather.gov/gridpoints/TOP/32,81/forecast", "API погоды");
        
        // Тест API пород собак
        yield return TestAPIEndpoint("https://api.dogapi.dog/v2/breeds", "API пород собак");
    }
    
    private IEnumerator TestAPIEndpoint(string url, string apiName)
    {
        Debug.Log($"Тестирование {apiName}: {url}");
        
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 10;
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"{apiName} доступен и работает корректно");
        }
        else
        {
            Debug.LogError($"{apiName} недоступен: {request.error}");
        }
        
        request.Dispose();
    }
    
    private void OnDestroy()
    {
        // Отменяем все запросы при уничтожении компонента
        if (currentRequest != null)
        {
            currentRequest.Request.Abort();
            currentRequest.Request.Dispose();
        }
        
        foreach (var item in requestQueue)
        {
            item.Request.Abort();
            item.Request.Dispose();
        }
        
        requestQueue.Clear();
    }
}