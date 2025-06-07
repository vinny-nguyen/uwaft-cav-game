using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeMap.Tutorial
{
    /// <summary>
    /// Core tutorial manager that orchestrates the tutorial flow
    /// </summary>
    public class NodeMapTutorialManager : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Tutorial Settings")]
        [SerializeField] private bool showTutorialOnStart = true;
        [SerializeField] private string playerPrefKey = "CompletedTutorial";

        [Header("Target References")]
        [SerializeField] private Transform firstNodeTransform;
        [SerializeField] private Transform finalNodeTransform;
        [SerializeField] private Transform driveButton;
        [SerializeField] private Transform homeButton;

        [Header("Component References")]
        [SerializeField] private TutorialUIController uiController;
        [SerializeField] private TutorialAudioManager audioManager;
        #endregion

        #region Private Fields
        private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
        private int currentStep = 0;
        private bool tutorialActive = false;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            SetupTutorialSteps();
            InitializeComponents();
        }

        private void Start()
        {
            // For testing - remove in production
            // ResetTutorialStatus();
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            if (uiController == null)
                uiController = GetComponent<TutorialUIController>();
            if (audioManager == null)
                audioManager = GetComponent<TutorialAudioManager>();
        }

        private void SetupTutorialSteps()
        {
            tutorialSteps.Clear();

            tutorialSteps.Add(new TutorialStep(
                "These are nodes. Click on them to learn about cars!",
                firstNodeTransform,
                false,
                new Vector2(16.13f, 7.28f),
                0f
            ));

            tutorialSteps.Add(new TutorialStep(
                "This is your goal. Complete all nodes to reach the final destination!",
                finalNodeTransform,
                false,
                new Vector2(-233.94f, -15.61f),
                78.79f
            ));

            tutorialSteps.Add(new TutorialStep(
                "Click this button to drive when you have completed a node!",
                driveButton,
                true,
                new Vector2(-105.51f, 52.11f),
                80.61f
            ));

            tutorialSteps.Add(new TutorialStep(
                "Click this button to go back to the main menu.",
                homeButton,
                true,
                new Vector2(137.57f, -216.42f),
                270.95f
            ));
        }
        #endregion

        #region Public API
        public void TriggerNodeReachedTutorial()
        {
            if (!HasCompletedTutorial())
                StartTutorial();
        }

        public void StartTutorial()
        {
            if (tutorialSteps.Count == 0)
            {
                Debug.LogWarning("No tutorial steps defined!");
                return;
            }

            tutorialActive = true;
            currentStep = 0;

            audioManager?.PlayTutorialStartSound();
            uiController?.StartTutorial();
            
            StartCoroutine(ShowCurrentStepCoroutine());
        }

        public void EndTutorial()
        {
            tutorialActive = false;
            
            audioManager?.PlayTutorialEndSound();
            uiController?.EndTutorial();

            PlayerPrefs.SetInt(playerPrefKey, 1);
            PlayerPrefs.Save();
        }

        public bool HasCompletedTutorial()
        {
            return PlayerPrefs.GetInt(playerPrefKey, 0) == 1;
        }

        public void ForceStartTutorial()
        {
            StartTutorial();
        }

        public void ResetTutorialStatus()
        {
            PlayerPrefs.DeleteKey(playerPrefKey);
            PlayerPrefs.Save();
        }

        public bool IsTutorialActive()
        {
            return tutorialActive;
        }
        #endregion

        #region Tutorial Step Management
        public void AdvanceToNextStep()
        {
            audioManager?.PlayStepAdvanceSound();
            currentStep++;

            if (currentStep >= tutorialSteps.Count)
                EndTutorial();
            else
                StartCoroutine(ShowCurrentStepCoroutine());
        }

        private IEnumerator ShowCurrentStepCoroutine()
        {
            if (currentStep >= tutorialSteps.Count)
            {
                EndTutorial();
                yield break;
            }

            TutorialStep step = tutorialSteps[currentStep];
            yield return uiController?.ShowStep(step);
        }

        public TutorialStep GetCurrentStep()
        {
            if (currentStep >= 0 && currentStep < tutorialSteps.Count)
                return tutorialSteps[currentStep];
            return null;
        }
        #endregion
    }
}