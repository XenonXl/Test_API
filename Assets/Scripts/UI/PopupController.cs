using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PopupController : MonoBehaviour
{
    [Header("Popup компоненты")]
    [SerializeField] private GameObject popup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private Button closeButton;
    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePopup);
        }
        
        // Изначально скрываем popup
        if (popup != null)
        {
            popup.SetActive(false);
        }
        ValidateComponents();
    }
    public void ShowPopup(string title, string content)
    {
        try
        {
            titleText.text = title ?? "Без названия";
            contentText.text = content ?? "Нет описания";
            
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentText.rectTransform);
            
            popup.SetActive(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при показе popup: {e.Message}");
        }
    }
    public void HidePopup()
    {
        if (popup != null)
        {
            popup.SetActive(false);
        }
    }
    private void ValidateComponents()
    {
        if (popup == null) Debug.LogError("Не назначен объект popup!");
        if (titleText == null) Debug.LogError("Не назначен компонент titleText!");
        if (contentText == null) Debug.LogError("Не назначен компонент contentText!");
        if (closeButton == null) Debug.LogError("Не назначен компонент closeButton!");
    }
    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HidePopup);
        }
    }
}