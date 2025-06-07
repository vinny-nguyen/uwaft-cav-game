using UnityEngine;
using UnityEngine.UI;

namespace NodeMap.Tutorial
{
    /// <summary>
    /// Manages the spotlight effect for highlighting tutorial targets
    /// </summary>
    public class TutorialSpotlightEffect : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Spotlight Settings")]
        [SerializeField] private Image dimmerPanel;
        [SerializeField] private float spotlightRadius = 100f;
        [SerializeField] private Material spotlightMaterial;
        [SerializeField] private float spotlightPulseSpeed = 1f;
        [SerializeField] private float spotlightPulseMagnitude = 0.1f;
        #endregion

        #region Private Fields
        private Camera mainCamera;
        private float pulseTimer = 0f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            if (dimmerPanel != null && dimmerPanel.gameObject.activeInHierarchy)
            {
                pulseTimer += Time.deltaTime;
            }
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            mainCamera = Camera.main;

            if (dimmerPanel != null && spotlightMaterial != null)
            {
                dimmerPanel.material = Instantiate(spotlightMaterial);
                dimmerPanel.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Public API
        public void StartEffect()
        {
            if (dimmerPanel != null)
            {
                dimmerPanel.gameObject.SetActive(true);
                dimmerPanel.color = new Color(0, 0, 0, 0);
                pulseTimer = 0f;
            }
        }

        public void EndEffect()
        {
            if (dimmerPanel != null)
                dimmerPanel.gameObject.SetActive(false);
        }

        public void SetAlpha(float alpha)
        {
            if (dimmerPanel != null)
                dimmerPanel.color = new Color(0, 0, 0, alpha);
        }

        public void UpdateSpotlight(Transform target, bool isUIElement, Canvas tutorialCanvas = null)
        {
            if (dimmerPanel == null || dimmerPanel.material == null || target == null || mainCamera == null)
                return;

            Vector2 viewportPosition = isUIElement
                ? GetViewportPositionForUIElement(target, tutorialCanvas)
                : mainCamera.WorldToViewportPoint(target.position);

            float sinPulse = Mathf.Sin(pulseTimer * spotlightPulseSpeed);
            float baseNormalizedRadius = spotlightRadius / Screen.height;
            float radiusPulseOffset = sinPulse * baseNormalizedRadius * spotlightPulseMagnitude;
            float actualNormalizedRadius = baseNormalizedRadius + radiusPulseOffset;
            
            actualNormalizedRadius = Mathf.Max(actualNormalizedRadius, baseNormalizedRadius * (1.0f - spotlightPulseMagnitude * 0.9f));
            actualNormalizedRadius = Mathf.Max(actualNormalizedRadius, 0.01f);

            dimmerPanel.material.SetFloat("_Radius", actualNormalizedRadius);
            dimmerPanel.material.SetFloat("_SoftEdge", actualNormalizedRadius * 0.2f);

            float aspectRatio = (float)Screen.width / Screen.height;
            dimmerPanel.material.SetVector("_Center", new Vector4(viewportPosition.x, viewportPosition.y, 0, 0));
            dimmerPanel.material.SetFloat("_AspectRatio", aspectRatio);
        }
        #endregion

        #region Private Methods
        private Vector2 GetViewportPositionForUIElement(Transform target, Canvas tutorialCanvas)
        {
            RectTransform targetRect = target.GetComponent<RectTransform>();
            if (targetRect == null) 
            {
                Debug.LogWarning($"Target {target.name} does not have a RectTransform component!");
                return Vector2.zero;
            }

            // Try to get the canvas from the target's hierarchy first
            Canvas targetCanvas = target.GetComponentInParent<Canvas>();
            
            // If no canvas found in target hierarchy, use the provided tutorial canvas
            if (targetCanvas == null)
                targetCanvas = tutorialCanvas;
                
            if (targetCanvas == null)
            {
                Debug.LogWarning("No canvas found for UI element spotlight positioning!");
                return Vector2.zero;
            }

            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);
            Vector3 targetCenter = (corners[0] + corners[2]) / 2;

            Camera uiCamera = targetCanvas.renderMode == RenderMode.ScreenSpaceCamera ? targetCanvas.worldCamera : null;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, targetCenter);

            return new Vector2(
                screenPoint.x / Screen.width,
                screenPoint.y / Screen.height
            );
        }
        #endregion
    }
}
