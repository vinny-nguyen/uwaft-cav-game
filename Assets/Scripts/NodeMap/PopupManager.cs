using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    public GameObject popupCanvas;
    public FadeManager fadeManager;
    public Button closeButton; // Assign in Inspector

    private bool isPopupOpen = false;

    public bool IsPopupOpen => isPopupOpen;

    void Start()
    {
        popupCanvas.SetActive(false);
        closeButton.onClick.AddListener(ClosePopup);
    }

    public void ShowPopup()
    {
        if (popupCanvas != null && fadeManager != null)
        {
            popupCanvas.SetActive(true);
            StartCoroutine(fadeManager.Fade(0, 1, 0.5f));
            isPopupOpen = true;
        }
    }

    public void ClosePopup()
    {
        if (popupCanvas != null && fadeManager != null)
        {
            StartCoroutine(fadeManager.Fade(1, 0, 0.5f, () =>
            {
                popupCanvas.SetActive(false);
                isPopupOpen = false;
                FindAnyObjectByType<PlayerMovement>().UpdateButtonVisibility();
            }));
        }
    }
}