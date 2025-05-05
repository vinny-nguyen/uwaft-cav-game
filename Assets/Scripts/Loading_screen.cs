using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loading_screen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private Text progressText;
    [SerializeField] private Text tipText; // Add a Text element for tips

    [Header("Settings")]
    [SerializeField] private float minimumLoadTime = 5f; // Increased minimum time
    [SerializeField] private string[] loadingTips; // Add your tips in Inspector

    private AsyncOperation loadingOperation;
    private float loadingProgress;
    private float timeLoading;
    private string targetSceneName;
    private bool loadingComplete = false;

    private void Start()
    {
        // Get target scene from PlayerPrefs
        targetSceneName = PlayerPrefs.GetString("TargetScene", "NodeMapFullHD");

        // Show random tip
        if (tipText != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }

        // Initialize loading
        loadingOperation = SceneManager.LoadSceneAsync(targetSceneName);
        loadingOperation.allowSceneActivation = false;

        // Setup UI
        if (loadingSlider != null)
        {
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = 1f;
            loadingSlider.value = 0f;
        }

        loadingProgress = 0f;
        timeLoading = 0f;
        loadingComplete = false;
    }

    private void Update()
    {
        // Track time
        timeLoading += Time.deltaTime;

        // Calculate real progress (0-0.9 range)
        float realProgress = Mathf.Clamp01(loadingOperation.progress / 0.9f);

        // Calculate fake progress based on time
        float fakeProgress = Mathf.Clamp01(timeLoading / minimumLoadTime);

        // Use whichever progress is slower
        loadingProgress = Mathf.Min(realProgress, fakeProgress);

        // Update UI
        if (loadingSlider != null)
            loadingSlider.value = loadingProgress;

        if (progressText != null)
            progressText.text = $"LOADING... {(loadingProgress * 100):0}%";

        // Check if we should transition
        if (timeLoading >= minimumLoadTime && realProgress >= 0.9f && !loadingComplete)
        {
            loadingComplete = true;
            loadingOperation.allowSceneActivation = true;
        }
    }
}