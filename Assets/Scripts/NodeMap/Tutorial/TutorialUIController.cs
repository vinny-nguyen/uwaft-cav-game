using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NodeMap.Tutorial
{
    /// <summary>
    /// Manages tutorial UI positioning, animations, and visual effects
    /// </summary>
    public class TutorialUIController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI References")]
        [SerializeField] private CanvasGroup tutorialCanvasGroup;
        [SerializeField] private RectTransform arrowImage;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI continueText;
        [SerializeField] private Image clickBlocker;
        [SerializeField] private RectTransform messagePanel;

        [Header("Animation Settings")]
        [SerializeField] private float arrowBobAmount = 20f;
        [SerializeField] private float arrowBobSpeed = 2f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float typewriterSpeed = 0.1f;
        #endregion

        #region Private Fields
        private NodeMapTutorialManager tutorialManager;
        private TutorialSpotlightEffect spotlightEffect;
        private Camera mainCamera;
        private Coroutine arrowAnimationCoroutine;
        private Coroutine typeMessageCoroutine;
        private bool isTypingMessage = false;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            UpdateArrowPosition();
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            mainCamera = Camera.main;
            tutorialManager = GetComponent<NodeMapTutorialManager>();
            spotlightEffect = GetComponent<TutorialSpotlightEffect>();

            if (tutorialCanvasGroup != null)
            {
                tutorialCanvasGroup.alpha = 0f;
                tutorialCanvasGroup.gameObject.SetActive(false);
            }

            if (continueText != null)
                continueText.gameObject.SetActive(false);
        }
        #endregion

        #region Public API
        public void StartTutorial()
        {
            tutorialCanvasGroup.gameObject.SetActive(true);

            if (clickBlocker != null)
            {
                clickBlocker.gameObject.SetActive(true);
                clickBlocker.color = new Color(0, 0, 0, 0.01f);
            }

            StartCoroutine(FadeInTutorial());
        }

        public void EndTutorial()
        {
            if (arrowAnimationCoroutine != null)
                StopCoroutine(arrowAnimationCoroutine);
            if (typeMessageCoroutine != null)
                StopCoroutine(typeMessageCoroutine);

            if (clickBlocker != null)
                clickBlocker.gameObject.SetActive(false);

            StartCoroutine(FadeOutTutorial());
        }

        public IEnumerator ShowStep(TutorialStep step)
        {
            if (continueText != null)
                continueText.gameObject.SetActive(false);

            // Animate text
            if (messageText != null)
            {
                if (typeMessageCoroutine != null)
                    StopCoroutine(typeMessageCoroutine);
                typeMessageCoroutine = StartCoroutine(AnimateTextMessage(step.Message));
            }

            // Position arrow and effects
            if (step.Target != null)
            {
                PositionArrowAtTarget(step.Target, step.IsUIElement, step.ArrowOffset, step.ArrowRotation);
                
                // Pass the canvas reference to spotlight effect
                Canvas canvas = tutorialCanvasGroup.GetComponentInParent<Canvas>();
                spotlightEffect?.UpdateSpotlight(step.Target, step.IsUIElement, canvas);

                if (arrowAnimationCoroutine != null)
                    StopCoroutine(arrowAnimationCoroutine);
                arrowAnimationCoroutine = StartCoroutine(AnimateArrow());
            }

            yield return StartCoroutine(WaitForClick());
        }
        #endregion

        #region Animation & Positioning
        private void UpdateArrowPosition()
        {
            if (!tutorialManager.IsTutorialActive())
                return;

            TutorialStep currentStep = tutorialManager.GetCurrentStep();
            if (currentStep?.Target != null)
            {
                PositionArrowAtTarget(
                    currentStep.Target,
                    currentStep.IsUIElement,
                    currentStep.ArrowOffset,
                    currentStep.ArrowRotation
                );
                
                // Pass the canvas reference to spotlight effect
                Canvas canvas = tutorialCanvasGroup.GetComponentInParent<Canvas>();
                spotlightEffect?.UpdateSpotlight(currentStep.Target, currentStep.IsUIElement, canvas);
            }
        }

        private void PositionArrowAtTarget(Transform target, bool isUIElement, Vector2 customOffset, float arrowRotation)
        {
            if (target == null || mainCamera == null || arrowImage == null)
                return;

            Canvas canvas = tutorialCanvasGroup.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
            if (canvasRectTransform == null) return;

            Vector2 localPosition = GetLocalPositionForTarget(target, isUIElement, canvas, canvasRectTransform);
            Vector2 defaultOffset = isUIElement ? new Vector2(0, 100) : new Vector2(100, 200);

            arrowImage.anchoredPosition = localPosition + defaultOffset + customOffset;
            arrowImage.localEulerAngles = new Vector3(0, 0, arrowRotation);

            if (messagePanel != null)
                PositionMessagePanelNearArrow();
        }

        private Vector2 GetLocalPositionForTarget(Transform target, bool isUIElement, Canvas canvas, RectTransform canvasRectTransform)
        {
            if (isUIElement)
                return GetLocalPositionForUIElement(target, canvas, canvasRectTransform);
            else
                return GetLocalPositionForWorldObject(target, canvas, canvasRectTransform);
        }

        private Vector2 GetLocalPositionForUIElement(Transform target, Canvas canvas, RectTransform canvasRectTransform)
        {
            RectTransform targetRect = target.GetComponent<RectTransform>();
            if (targetRect == null) return Vector2.zero;

            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);
            Vector3 targetCenter = (corners[0] + corners[2]) / 2;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetCenter);
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform, screenPoint, canvas.worldCamera, out localPosition);

            return localPosition;
        }

        private Vector2 GetLocalPositionForWorldObject(Transform target, Canvas canvas, RectTransform canvasRectTransform)
        {
            Vector2 localPosition = Vector2.zero;

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector2 viewportPosition = mainCamera.WorldToViewportPoint(target.position);
                Vector2 screenPosition = new Vector2(
                    viewportPosition.x * Screen.width,
                    viewportPosition.y * Screen.height
                );

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform, screenPosition, null, out localPosition);
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, target.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform, screenPoint, canvas.worldCamera, out localPosition);
            }

            return localPosition;
        }

        private void PositionMessagePanelNearArrow()
        {
            messagePanel.anchoredPosition = arrowImage.anchoredPosition + new Vector2(0, -120);
            KeepRectTransformOnScreen(messagePanel, tutorialCanvasGroup.GetComponent<RectTransform>(), 20);
        }

        private void KeepRectTransformOnScreen(RectTransform rt, RectTransform container, float padding)
        {
            if (rt == null || container == null) return;

            Vector2 size = rt.rect.size;
            Vector2 containerSize = container.rect.size;

            float minX = -containerSize.x / 2 + size.x / 2 + padding;
            float maxX = containerSize.x / 2 - size.x / 2 - padding;
            float minY = -containerSize.y / 2 + size.y / 2 + padding;
            float maxY = containerSize.y / 2 - size.y / 2 - padding;

            rt.anchoredPosition = new Vector2(
                Mathf.Clamp(rt.anchoredPosition.x, minX, maxX),
                Mathf.Clamp(rt.anchoredPosition.y, minY, maxY)
            );
        }

        private IEnumerator AnimateArrow()
        {
            Vector2 originalPosition = arrowImage.anchoredPosition;
            float time = 0f;
            float initialY = originalPosition.y;

            while (tutorialManager.IsTutorialActive())
            {
                time += Time.deltaTime;
                float yOffset = Mathf.Sin(time * arrowBobSpeed) * arrowBobAmount;
                arrowImage.anchoredPosition = new Vector2(arrowImage.anchoredPosition.x, initialY + yOffset);
                yield return null;
            }
        }

        private IEnumerator AnimateTextMessage(string message)
        {
            if (messageText == null) yield break;

            isTypingMessage = true;
            messageText.text = "";
            
            foreach (char letter in message.ToCharArray())
            {
                messageText.text += letter;
                yield return new WaitForSeconds(typewriterSpeed);
            }

            if (continueText != null)
                continueText.gameObject.SetActive(true);

            isTypingMessage = false;
        }

        private IEnumerator WaitForClick()
        {
            yield return new WaitForSeconds(0.2f);

            while (tutorialManager.IsTutorialActive())
            {
                if (Input.GetMouseButtonDown(0) && !isTypingMessage)
                {
                    tutorialManager.AdvanceToNextStep();
                    break;
                }
                yield return null;
            }
        }

        private IEnumerator FadeInTutorial()
        {
            tutorialCanvasGroup.alpha = 0f;
            if (continueText != null) continueText.gameObject.SetActive(false);
            if (messageText != null) messageText.text = "";

            spotlightEffect?.StartEffect();

            float timer = 0f;
            while (timer < fadeInDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / fadeInDuration);
                tutorialCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                spotlightEffect?.SetAlpha(Mathf.Lerp(0, 0.7f, t));
                yield return null;
            }

            tutorialCanvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutTutorial()
        {
            float timer = 0f;
            if (continueText != null) continueText.gameObject.SetActive(false);

            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / fadeOutDuration);
                tutorialCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                spotlightEffect?.SetAlpha(Mathf.Lerp(0.7f, 0f, t));
                yield return null;
            }

            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.gameObject.SetActive(false);
            spotlightEffect?.EndEffect();
        }
        #endregion
    }
}
