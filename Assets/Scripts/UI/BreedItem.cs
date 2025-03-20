// BreedItem.cs - Элемент породы собаки
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;

public class BreedItem : MonoBehaviour
{
    [SerializeField] public TMP_Text nameText;
    [SerializeField] public Button button;
    
    public string breedId;
    public Action<string> onBreedSelected;
    
    // Инициализация элемента
    public void Initialize(int index, string name, string id, Action<string> onSelected)
    {
        nameText.text = $"{index} - {name}";
        breedId = id;
        onBreedSelected = onSelected;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClicked);
    }
    
    // Обработка нажатия на кнопку
    public void OnButtonClicked()
    {
        onBreedSelected?.Invoke(breedId);
    }
    
    // Фабрика для создания элементов через Zenject
    public class Factory : PlaceholderFactory<Transform, BreedItem>
    {
    }
}