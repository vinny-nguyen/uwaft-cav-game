using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace NodeMap
{
    public class CarUpgradeManager : MonoBehaviour
    {
        #region Types
        [System.Serializable]
        public class CarUpgrade
        {
            public string upgradeName;
            public Sprite carBodySprite;
            public Sprite wheelSprite;
            [TextArea]
            public string upgradeDescription;
        }
        #endregion

        #region Inspector Fields

        [Header("Debug")]
        [Tooltip("Enable to show debug logs")]
        [SerializeField] private bool showDebugLogs = false;

        [Header("Initial Appearance")]
        [SerializeField] private Sprite initialCarBodySprite;
        [SerializeField] private Sprite initialWheelSprite;
        [SerializeField] private bool useInitialAppearance = true;

        [Header("Upgrade Settings")]
        [SerializeField] private List<CarUpgrade> carUpgrades = new();

        [Header("Car References")]
        [SerializeField] private SpriteRenderer carBodyRenderer;
        [SerializeField] private SpriteRenderer frontWheelRenderer;
        [SerializeField] private SpriteRenderer rearWheelRenderer;

        [Header("Animation")]
        [SerializeField] private float upgradeAnimationDuration = 1.0f;
        [SerializeField] private ParticleSystem upgradeParticles;

        #endregion

        #region Unity Events

        private void Start()
        {
            DebugLog("CarUpgradeManager started");

            if (NopeMapManager.Instance != null)
            {
                NopeMapManager.Instance.OnNodeCompleted += HandleNodeCompleted;
                DebugLog("Subscribed to node completion events");
            }
            else
            {
                DebugLog("Warning: NopeMapManager instance is null", LogType.Warning);
            }

            UpdateCarAppearance();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Subscription

        private void UnsubscribeFromEvents()
        {
            if (NopeMapManager.Instance != null)
            {
                NopeMapManager.Instance.OnNodeCompleted -= HandleNodeCompleted;
                DebugLog("Unsubscribed from node completion events");
            }
        }

        #endregion

        #region Upgrade Logic

        private void UpdateCarAppearance()
        {
            DebugLog("Updating car appearance");

            // Get the current appropriate upgrade based on highest completed node
            int highestCompletedNode = NopeMapManager.Instance?.HighestCompletedNodeIndex ?? -1;
            int upgradeIndex = Mathf.Min(highestCompletedNode, carUpgrades.Count - 1);

            if (upgradeIndex >= 0 && carUpgrades.Count > 0)
            {
                DebugLog($"Applying upgrade {upgradeIndex}: {carUpgrades[upgradeIndex].upgradeName}");
                ApplyUpgrade(upgradeIndex, false);
            }
            else if (useInitialAppearance)
            {
                DebugLog("Applying initial appearance");
                ApplyInitialAppearance();
            }
            else if (carUpgrades.Count > 0)
            {
                DebugLog("Applying default first upgrade");
                ApplyUpgrade(0, false);
            }
        }

        private void HandleNodeCompleted(int nodeIndex)
        {
            DebugLog($"Node {nodeIndex} completed");

            if (!gameObject || !isActiveAndEnabled)
                return;

            // Only trigger upgrade if this is the new highest completed node
            if (nodeIndex == NopeMapManager.Instance.HighestCompletedNodeIndex)
            {
                int newUpgradeIndex = Mathf.Min(nodeIndex, carUpgrades.Count - 1);
                DebugLog($"New highest node completed - upgrading car to level {newUpgradeIndex}");
                UpgradeCar(newUpgradeIndex);
            }
            else
            {
                DebugLog("Not highest node - no upgrade needed");
            }
        }

        private void ApplyInitialAppearance()
        {
            DebugLog("Applying initial car appearance");

            if (carBodyRenderer != null && initialCarBodySprite != null)
                carBodyRenderer.sprite = initialCarBodySprite;

            if (frontWheelRenderer != null && initialWheelSprite != null)
                frontWheelRenderer.sprite = initialWheelSprite;

            if (rearWheelRenderer != null && initialWheelSprite != null)
                rearWheelRenderer.sprite = initialWheelSprite;
        }

        public void UpgradeCar(int upgradeIndex)
        {
            DebugLog($"UpgradeCar({upgradeIndex}) called");

            if (!gameObject || !isActiveAndEnabled)
            {
                DebugLog("Cannot upgrade car - GameObject inactive", LogType.Warning);
                return;
            }

            // Keep this check as it's for direct external calls to UpgradeCar
            if (upgradeIndex < 0 || upgradeIndex >= carUpgrades.Count)
            {
                DebugLog($"Invalid upgrade index: {upgradeIndex}", LogType.Error);
                return;
            }

            try
            {
                DebugLog($"Starting animation for upgrade to {carUpgrades[upgradeIndex].upgradeName}");
                StartCoroutine(AnimateUpgrade(upgradeIndex));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to upgrade car: {e.Message}");
                DebugLog("Falling back to direct upgrade without animation", LogType.Warning);
                ApplyUpgrade(upgradeIndex, true);
            }
        }

        private void ApplyUpgrade(int upgradeIndex, bool showParticles = true)
        {
            // This safety check is still needed as ApplyUpgrade can be called directly
            if (upgradeIndex < 0 || upgradeIndex >= carUpgrades.Count)
            {
                DebugLog($"Cannot apply upgrade - Invalid index: {upgradeIndex}", LogType.Error);
                return;
            }

            CarUpgrade upgrade = carUpgrades[upgradeIndex];
            DebugLog($"Applying upgrade: {upgrade.upgradeName}");

            if (carBodyRenderer != null && upgrade.carBodySprite != null)
                carBodyRenderer.sprite = upgrade.carBodySprite;

            if (frontWheelRenderer != null && upgrade.wheelSprite != null)
                frontWheelRenderer.sprite = upgrade.wheelSprite;

            if (rearWheelRenderer != null && upgrade.wheelSprite != null)
                rearWheelRenderer.sprite = upgrade.wheelSprite;

            if (showParticles && upgradeParticles != null)
            {
                DebugLog("Playing upgrade particles");
                upgradeParticles.Play();
            }
        }

        #endregion

        #region Animation

        private IEnumerator AnimateUpgrade(int upgradeIndex)
        {
            DebugLog("Starting upgrade animation");
            Vector3 originalBodyScale = carBodyRenderer.transform.localScale;
            Vector3 originalFrontWheelScale = frontWheelRenderer.transform.localScale;
            Vector3 originalRearWheelScale = rearWheelRenderer.transform.localScale;

            float halfDuration = upgradeAnimationDuration / 2f;

            // First phase - scale down and fade
            DebugLog("Animation phase 1: Scale down");
            for (float t = 0; t < halfDuration; t += Time.deltaTime)
            {
                float progress = t / halfDuration;
                float scale = Mathf.Lerp(1f, 0.8f, progress);
                float alpha = Mathf.Lerp(1f, 0.5f, progress);

                ApplyScaleAndOpacity(scale, alpha, originalBodyScale, originalFrontWheelScale, originalRearWheelScale);
                yield return null;
            }

            // Apply the upgrade
            DebugLog("Animation midpoint: Applying new sprites");
            ApplyUpgrade(upgradeIndex, true);

            // Second phase - scale back up
            DebugLog("Animation phase 2: Scale up");
            for (float t = 0; t < halfDuration; t += Time.deltaTime)
            {
                float progress = t / halfDuration;
                float scale = Mathf.Lerp(0.8f, 1f, progress);
                float alpha = Mathf.Lerp(0.5f, 1f, progress);

                ApplyScaleAndOpacity(scale, alpha, originalBodyScale, originalFrontWheelScale, originalRearWheelScale);
                yield return null;
            }

            // Ensure final state is perfect
            ApplyScaleAndOpacity(1f, 1f, originalBodyScale, originalFrontWheelScale, originalRearWheelScale);
            DebugLog("Animation complete");
        }

        private void ApplyScaleAndOpacity(float scale, float alpha, Vector3 originalBodyScale, Vector3 originalFrontWheelScale, Vector3 originalRearWheelScale)
        {
            carBodyRenderer.transform.localScale = originalBodyScale * scale;
            frontWheelRenderer.transform.localScale = originalFrontWheelScale * scale;
            rearWheelRenderer.transform.localScale = originalRearWheelScale * scale;

            Color bodyColor = carBodyRenderer.color;
            bodyColor.a = alpha;
            carBodyRenderer.color = bodyColor;

            Color wheelColor = frontWheelRenderer.color;
            wheelColor.a = alpha;
            frontWheelRenderer.color = wheelColor;
            rearWheelRenderer.color = wheelColor;
        }

        #endregion

        #region Debug Helper

        private void DebugLog(string message, LogType logType = LogType.Log)
        {
            if (!showDebugLogs) return;

            switch (logType)
            {
                case LogType.Warning:
                    Debug.LogWarning($"[CarUpgrader] {message}");
                    break;
                case LogType.Error:
                    Debug.LogError($"[CarUpgrader] {message}");
                    break;
                default:
                    Debug.Log($"[CarUpgrader] {message}");
                    break;
            }
        }

        #endregion
    }
}