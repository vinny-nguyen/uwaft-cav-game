using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace NodeMap
{
    /// <summary>
    /// Manages car visual upgrades as nodes are completed
    /// </summary>
    public class CarUpgradeManager : MonoBehaviour
    {
        [System.Serializable]
        public class CarUpgrade
        {
            public string upgradeName;
            public Sprite carBodySprite;
            public Sprite wheelSprite;
            [TextArea]
            public string upgradeDescription;
        }

        [Header("Initial Appearance")]
        [SerializeField] private Sprite initialCarBodySprite;
        [SerializeField] private Sprite initialWheelSprite;
        [SerializeField] private bool useInitialAppearance = true;

        [Header("Upgrade Settings")]
        [SerializeField] private List<CarUpgrade> carUpgrades = new List<CarUpgrade>();

        [Header("Car References")]
        [SerializeField] private SpriteRenderer carBodyRenderer;
        [SerializeField] private SpriteRenderer frontWheelRenderer;
        [SerializeField] private SpriteRenderer rearWheelRenderer;

        [Header("Animation")]
        [SerializeField] private float upgradeAnimationDuration = 1.0f;
        [SerializeField] private ParticleSystem upgradeParticles;

        private int currentUpgradeIndex = -1; // -1 means initial state

        private void Start()
        {
            // Apply initial appearance
            if (useInitialAppearance)
            {
                ApplyInitialAppearance();
            }
            else if (carUpgrades.Count > 0)
            {
                // Fallback to first upgrade if initial appearance not used
                ApplyUpgrade(0, false);
                currentUpgradeIndex = 0;
            }

            // Subscribe to node completion events
            NopeMapManager gameManager = FindFirstObjectByType<NopeMapManager>();
            if (gameManager != null)
            {
                gameManager.OnNodeCompleted += HandleNodeCompleted;
            }
        }

        /// <summary>
        /// Applies the initial car appearance before any upgrades
        /// </summary>
        private void ApplyInitialAppearance()
        {
            // Set initial car body
            if (carBodyRenderer != null && initialCarBodySprite != null)
            {
                carBodyRenderer.sprite = initialCarBodySprite;
            }

            // Set initial wheels
            if (frontWheelRenderer != null && initialWheelSprite != null)
            {
                frontWheelRenderer.sprite = initialWheelSprite;
            }

            if (rearWheelRenderer != null && initialWheelSprite != null)
            {
                rearWheelRenderer.sprite = initialWheelSprite;
            }

            currentUpgradeIndex = -1; // Indicate we're using initial appearance

            Debug.Log("Applied initial car appearance");
        }

        /// <summary>
        /// Handles node completion event
        /// </summary>
        private void HandleNodeCompleted(int nodeIndex)
        {
            // Check if we have an upgrade for this node
            int upgradeIndex = GetUpgradeIndexForNode(nodeIndex);

            if (upgradeIndex >= 0 && upgradeIndex < carUpgrades.Count && upgradeIndex != currentUpgradeIndex)
            {
                UpgradeCar(upgradeIndex);
            }
        }

        /// <summary>
        /// Determines which upgrade to use based on node index
        /// </summary>
        private int GetUpgradeIndexForNode(int nodeIndex)
        {
            // Simple implementation: use node index as upgrade index
            // You could also use a mapping table or other logic

            // Make sure we don't go out of bounds
            return Mathf.Min(nodeIndex, carUpgrades.Count - 1);
        }

        /// <summary>
        /// Upgrades the car with animation
        /// </summary>
        public void UpgradeCar(int upgradeIndex)
        {
            if (upgradeIndex < 0 || upgradeIndex >= carUpgrades.Count)
                return;

            // Start upgrade animation
            StartCoroutine(AnimateUpgrade(upgradeIndex));

            // Update current index
            currentUpgradeIndex = upgradeIndex;

            Debug.Log($"Car upgraded to: {carUpgrades[upgradeIndex].upgradeName}");
        }

        /// <summary>
        /// Immediately applies an upgrade without animation
        /// </summary>
        private void ApplyUpgrade(int upgradeIndex, bool showParticles = true)
        {
            if (upgradeIndex < 0 || upgradeIndex >= carUpgrades.Count)
                return;

            CarUpgrade upgrade = carUpgrades[upgradeIndex];

            // Update sprites
            if (carBodyRenderer != null && upgrade.carBodySprite != null)
                carBodyRenderer.sprite = upgrade.carBodySprite;

            if (frontWheelRenderer != null && upgrade.wheelSprite != null)
                frontWheelRenderer.sprite = upgrade.wheelSprite;

            if (rearWheelRenderer != null && upgrade.wheelSprite != null)
                rearWheelRenderer.sprite = upgrade.wheelSprite;

            // Play particles if requested
            if (showParticles && upgradeParticles != null)
                upgradeParticles.Play();
        }

        /// <summary>
        /// Animates the car upgrade with visual effects
        /// </summary>
        private System.Collections.IEnumerator AnimateUpgrade(int upgradeIndex)
        {
            // Save original scales
            Vector3 originalBodyScale = carBodyRenderer.transform.localScale;
            Vector3 originalFrontWheelScale = frontWheelRenderer.transform.localScale;
            Vector3 originalRearWheelScale = rearWheelRenderer.transform.localScale;

            // Pulse animation
            float halfDuration = upgradeAnimationDuration / 2f;

            // Scale down and fade
            for (float t = 0; t < halfDuration; t += Time.deltaTime)
            {
                float progress = t / halfDuration;
                float scale = Mathf.Lerp(1f, 0.8f, progress);
                float alpha = Mathf.Lerp(1f, 0.5f, progress);

                // Scale down
                carBodyRenderer.transform.localScale = originalBodyScale * scale;
                frontWheelRenderer.transform.localScale = originalFrontWheelScale * scale;
                rearWheelRenderer.transform.localScale = originalRearWheelScale * scale;

                // Fade
                Color bodyColor = carBodyRenderer.color;
                bodyColor.a = alpha;
                carBodyRenderer.color = bodyColor;

                Color wheelColor = frontWheelRenderer.color;
                wheelColor.a = alpha;
                frontWheelRenderer.color = wheelColor;
                rearWheelRenderer.color = wheelColor;

                yield return null;
            }

            // Apply the new sprites while car is small
            ApplyUpgrade(upgradeIndex, true);

            // For first upgrade, show a special notification
            if (currentUpgradeIndex == -1)
            {
                // ShowFirstUpgradeNotification(upgradeIndex);
            }
            else
            {
                // ShowUpgradeNotification(upgradeIndex);
            }

            // Update current index
            currentUpgradeIndex = upgradeIndex;

            // Scale back up and restore opacity
            for (float t = 0; t < halfDuration; t += Time.deltaTime)
            {
                float progress = t / halfDuration;
                float scale = Mathf.Lerp(0.8f, 1f, progress);
                float alpha = Mathf.Lerp(0.5f, 1f, progress);

                // Scale up
                carBodyRenderer.transform.localScale = originalBodyScale * scale;
                frontWheelRenderer.transform.localScale = originalFrontWheelScale * scale;
                rearWheelRenderer.transform.localScale = originalRearWheelScale * scale;

                // Restore opacity
                Color bodyColor = carBodyRenderer.color;
                bodyColor.a = alpha;
                carBodyRenderer.color = bodyColor;

                Color wheelColor = frontWheelRenderer.color;
                wheelColor.a = alpha;
                frontWheelRenderer.color = wheelColor;
                rearWheelRenderer.color = wheelColor;

                yield return null;
            }

            // Ensure final state is correct
            carBodyRenderer.transform.localScale = originalBodyScale;
            frontWheelRenderer.transform.localScale = originalFrontWheelScale;
            rearWheelRenderer.transform.localScale = originalRearWheelScale;

            Color finalBodyColor = carBodyRenderer.color;
            finalBodyColor.a = 1f;
            carBodyRenderer.color = finalBodyColor;

            Color finalWheelColor = frontWheelRenderer.color;
            finalWheelColor.a = 1f;
            frontWheelRenderer.color = finalWheelColor;
            rearWheelRenderer.color = finalWheelColor;
        }
    }
}