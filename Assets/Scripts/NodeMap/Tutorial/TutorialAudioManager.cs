using UnityEngine;

namespace NodeMap.Tutorial
{
    /// <summary>
    /// Manages audio for tutorial events
    /// </summary>
    public class TutorialAudioManager : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip tutorialStartSound;
        [SerializeField] private AudioClip stepAdvanceSound;
        [SerializeField] private AudioClip tutorialEndSound;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeAudioSource();
        }
        #endregion

        #region Initialization
        private void InitializeAudioSource()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        #endregion

        #region Public API
        public void PlayTutorialStartSound()
        {
            PlaySound(tutorialStartSound);
        }

        public void PlayStepAdvanceSound()
        {
            PlaySound(stepAdvanceSound);
        }

        public void PlayTutorialEndSound()
        {
            PlaySound(tutorialEndSound);
        }
        #endregion

        #region Private Methods
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        #endregion
    }
}
