using System;
using UnityEngine;
using Newtonsoft.Json;
public class DogBreedsService
{
    private readonly RequestQueue requestQueue;
    private readonly string breedsUrl = "https://dogapi.dog/api/v2/breeds";
    private readonly string breedDetailsUrlFormat = "https://dogapi.dog/api/v2/breeds/{0}";
    private const string BREEDS_LIST_REQUEST_TYPE = "breeds_list";
    private const string BREED_DETAILS_REQUEST_TYPE = "breed_details";
    public DogBreedsService(RequestQueue requestQueue)
    {
        this.requestQueue = requestQueue;
    }
    // Получить список пород
    public void GetBreeds(Action<BreedsListResponse> onSuccess, Action<string> onError)
    {
        Debug.Log($"DogBreedsService: Запрашиваем список пород. URL: {breedsUrl}");
        requestQueue.AddRequest(
            breedsUrl,
            jsonResponse => {
                Debug.Log($"DogBreedsService: Получен JSON-ответ: {jsonResponse}");
                try {
                    var settings = new JsonSerializerSettings 
                    { 
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    
                    var data = JsonConvert.DeserializeObject<BreedsListResponse>(jsonResponse, settings);
                    if (data == null || data.data == null)
                    {
                        throw new Exception("Получены некорректные данные списка пород");
                    }
                    onSuccess?.Invoke(data);
                }
                catch (JsonException jsonEx) {
                    Debug.LogError($"Ошибка парсинга JSON списка пород: {jsonEx.Message}");
                    onError?.Invoke($"Ошибка формата данных списка пород: {jsonEx.Message}");
                }
                catch (Exception e) {
                    Debug.LogError($"Общая ошибка получения списка пород: {e.Message}");
                    onError?.Invoke($"Ошибка получения списка пород: {e.Message}");
                }
            },
            errorMessage => onError?.Invoke(errorMessage),
            BREEDS_LIST_REQUEST_TYPE
        );
    }
    // Получить детали о породе
    public void GetBreedDetails(string breedId, Action<BreedResponse> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(breedId))
        {
            onError?.Invoke("ID породы отсутствует");
            return;
        }
        string url = string.Format(breedDetailsUrlFormat, breedId);
        Debug.Log($"Запрашиваем детали породы. URL: {url}");
        requestQueue.AddRequest(
            url,
            jsonResponse =>
            {
                try
                {
                    Debug.Log($"Получен JSON-ответ: {jsonResponse}");
                    
                    if (string.IsNullOrEmpty(jsonResponse))
                    {
                        throw new Exception("Получен пустой JSON-ответ");
                    }
                    var settings = new JsonSerializerSettings 
                    { 
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        Error = (sender, args) =>
                        {
                            Debug.LogError($"Ошибка десериализации: {args.ErrorContext.Error.Message}");
                            args.ErrorContext.Handled = true;
                        }
                    };
                    var response = JsonConvert.DeserializeObject<BreedResponse>(jsonResponse, settings);
                    
                    // Подробная проверка полученных данных
                    if (response == null)
                        throw new Exception("Ответ пуст");
                        
                    if (response.data == null)
                        throw new Exception("Данные отсутствуют в ответе");
                        
                    if (response.data.attributes == null)
                        throw new Exception("Атрибуты отсутствуют в данных");
                        
                    if (string.IsNullOrEmpty(response.data.attributes.name))
                        throw new Exception("Имя породы отсутствует");
                    Debug.Log($"Успешно получены данные о породе: {response.data.attributes.name}");
                    onSuccess?.Invoke(response);
                }
                catch (JsonException jsonEx)
                {
                    Debug.LogError($"Ошибка парсинга JSON: {jsonEx.Message}\nJSON: {jsonResponse}");
                    onError?.Invoke($"Ошибка формата данных: {jsonEx.Message}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Общая ошибка получения деталей породы: {e.Message}\nJSON: {jsonResponse}");
                    onError?.Invoke($"Ошибка получения деталей породы: {e.Message}");
                }
            },
            errorMessage => onError?.Invoke(errorMessage),
            BREED_DETAILS_REQUEST_TYPE
        );
    }
    private void ValidateBreedResponse(BreedResponse response)
    {
        if (response == null)
            throw new Exception("Не удалось десериализовать ответ");
        
        if (response.data == null)
            throw new Exception("Поле data в ответе отсутствует");
        
        if (response.data.attributes == null)
            throw new Exception("Поле attributes отсутствует");
        
        if (string.IsNullOrEmpty(response.data.attributes.name))
            throw new Exception("Имя породы отсутствует");
    }
    public void CancelBreedsListRequest()
    {
        requestQueue.CancelRequests(BREEDS_LIST_REQUEST_TYPE);
    }
    public void CancelBreedDetailsRequest()
    {
        requestQueue.CancelRequests(BREED_DETAILS_REQUEST_TYPE);
    }
    public void CancelAllBreedRequests()
    {
        CancelBreedsListRequest();
        CancelBreedDetailsRequest();
    }
}
// Добавьте этот класс для списка пород
[Serializable]
public class BreedsListResponse
{
    public BreedData[] data;
}