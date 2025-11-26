using UnityEngine;

public class ProfileMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject profilePanel;

    private bool isOpen;

    private void Start()
    {
        if (profilePanel != null)
            profilePanel.SetActive(false);
    }

    public void ToggleProfilePanel()
    {
        if (profilePanel == null) return;

        isOpen = !isOpen;
        profilePanel.SetActive(isOpen);
    }
}

