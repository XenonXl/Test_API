using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class DogBreedsTab : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Transform breedListContainer;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private GameObject breedItemPrefab;
    private DogBreedsService dogBreedsService;
    private PopupController popupController;
    private List<BreedItem> breedItems = new List<BreedItem>();
    private string pendingBreedId;
    [Inject]
    public void Construct(DogBreedsService dogBreedsService, PopupController popupController)
    {
        this.dogBreedsService = dogBreedsService;
        this.popupController = popupController;
    }
    private void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(LoadBreeds);
        }
        HideError();
    }
    public void Show()
    {
        rootPanel.SetActive(true);
        LoadBreeds();
    }
    public void Hide()
    {
        rootPanel.SetActive(false);
        dogBreedsService.CancelAllBreedRequests();
        HideLoading();
        HideError();
        popupController.HidePopup();
    }
    private void LoadBreeds()
    {
        Debug.Log("DogBreedsTab: Начинаем загрузку списка пород");
        ShowLoading();
        HideError();
        dogBreedsService.GetBreeds(OnBreedsReceived, OnBreedsError);
    }
    private void OnBreedsReceived(BreedsListResponse response)
    {
        Debug.Log($"DogBreedsTab: Получен ответ с породами. Данные: {(response?.data != null ? response.data.Length : 0)} пород");
        HideLoading();
        if (response?.data != null && response.data.Length > 0)
        {
            ClearBreedList();
            int count = Mathf.Min(response.data.Length, 10);
            Debug.Log($"DogBreedsTab: Отображаем {count} пород");
            for (int i = 0; i < count; i++)
            {
                var breedData = response.data[i];
                if (breedData?.attributes != null)
                {
                    CreateBreedItem(i + 1, breedData.attributes.name, breedData.id);
                }
            }
        }
        else
        {
            Debug.LogWarning("DogBreedsTab: Получен пустой список пород");
            ShowError("Не удалось загрузить список пород");
        }
    }
    private void CreateBreedItem(int index, string name, string id)
    {
        try
        {
            if (breedItemPrefab == null)
            {
                throw new Exception("Префаб BreedItem не назначен");
            }
            if (breedListContainer == null)
            {
                throw new Exception("Контейнер для списка пород не назначен");
            }
            GameObject breedItemObject = Instantiate(breedItemPrefab, breedListContainer);
            BreedItem item = breedItemObject.GetComponent<BreedItem>();
            if (item == null)
            {
                throw new Exception("Компонент BreedItem не найден на префабе");
            }
            item.Initialize(index, name, id, OnBreedSelected);
            breedItems.Add(item);
            Debug.Log($"Успешно создан элемент породы: {name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при создании элемента породы: {e.Message}\n{e.StackTrace}");
        }
    }
    private void OnBreedSelected(string breedId)
    {
        if (pendingBreedId == breedId) return;
        if (!string.IsNullOrEmpty(pendingBreedId))
        {
            dogBreedsService.CancelBreedDetailsRequest();
            HideLoading();
        }
        pendingBreedId = breedId;
        ShowLoading();
        HideError();
        dogBreedsService.GetBreedDetails(breedId, OnBreedDetailsReceived, OnBreedDetailsError);
    }
    private void OnBreedDetailsReceived(BreedResponse response)
    {
        try
        {
            HideLoading();
            string currentBreedId = pendingBreedId;
            pendingBreedId = null;
            if (response == null)
            {
                throw new System.Exception("Получен пустой ответ");
            }
            if (response.data == null)
            {
                throw new System.Exception("Отсутствуют данные в ответе");
            }
            if (response.data.attributes == null)
            {
                throw new System.Exception("Отсутствуют атрибуты породы");
            }
            string name = response.data.attributes.name ?? "Неизвестная порода";
            string description = response.data.attributes.description ?? "Описание отсутствует";
            // Формируем подробное описание
            string detailedDescription = $"{description}\n\n" +
                $"Продолжительность жизни: {response.data.attributes.life?.min}-{response.data.attributes.life?.max} лет\n" +
                $"Вес самцов: {response.data.attributes.male_weight?.min}-{response.data.attributes.male_weight?.max} кг\n" +
                $"Вес самок: {response.data.attributes.female_weight?.min}-{response.data.attributes.female_weight?.max} кг\n" +
                $"Гипоаллергенная: {(response.data.attributes.hypoallergenic ? "Да" : "Нет")}";
            Debug.Log($"Показываем информацию о породе: {name}");
            popupController.ShowPopup(name, detailedDescription);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при обработке данных о породе {pendingBreedId}: {e.Message}");
            ShowError("Не удалось загрузить информацию о породе");
        }
    }
    // Показать индикатор загрузки
    private void ShowLoading()
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }
    }
    
    // Скрыть индикатор загрузки
    private void HideLoading()
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
    }
    
    // Показать панель ошибки
    private void ShowError(string errorMessage)
    {
        if (errorPanel != null)
        {
            errorPanel.SetActive(true);
            
            // Если есть текстовый компонент для отображения ошибки
            TMPro.TMP_Text errorText = errorPanel.GetComponentInChildren<TMPro.TMP_Text>();
            if (errorText != null)
            {
                errorText.text = errorMessage;
            }
        }
    }
    
    // Скрыть панель ошибки
    private void HideError()
    {
        if (errorPanel != null)
        {
            errorPanel.SetActive(false);
        }
    }
    
    // Обработка ошибки получения списка пород
    private void OnBreedsError(string error)
    {
        Debug.LogError($"DogBreedsTab: Ошибка получения списка пород: {error}");
        HideLoading();
        ShowError($"Ошибка получения списка пород: {error}");
    }
    
    // Обработка ошибки получения деталей о породе
    private void OnBreedDetailsError(string error)
    {
        HideLoading();
        pendingBreedId = null;
        Debug.LogError($"Ошибка получения деталей о породе: {error}");
        ShowError($"Ошибка получения деталей о породе: {error}");
    }

    private void ClearBreedList()
    {
        foreach (var item in breedItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        breedItems.Clear();
    }
    private void OnDestroy()
    {
        ClearBreedList();
        dogBreedsService.CancelAllBreedRequests();
    }

}