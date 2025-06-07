using UnityEngine;
using System.Collections.Generic;

namespace NodeMap.Movement
{
    /// <summary>
    /// Generates and manages spline stops
    /// </summary>
    public class SplineStopGenerator : MonoBehaviour
    {
        [Header("Stop Configuration")]
        [SerializeField] private int numberOfStops = 6;
        [SerializeField] private List<Sprite> normalNodeSprites = new();

        private readonly List<SplineStop> stops = new();

        public List<SplineStop> GetStops() => stops;

        /// <summary>
        /// Generates spline stops based on configuration
        /// </summary>
        public void GenerateStops()
        {
            stops.Clear();
            
            for (int i = 1; i <= numberOfStops; i++)
            {
                float percent = i / 7f;
                SplineStop stop = new SplineStop
                {
                    splinePercent = percent,
                    offset = Vector3.zero
                };

                // If we have sprites available, assign them
                if (i - 1 < normalNodeSprites.Count)
                    stop.nodeSprite = normalNodeSprites[i - 1];

                stops.Add(stop);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Force regenerate stops for editor use
        /// </summary>
        public void ForceGenerateStops() => GenerateStops();
#endif
    }
}
