using UnityEngine;

namespace Commersion.Core.Loader
{
    public class UISpinner : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Spin speed in degrees per second.")]
        public float baseSpinSpeed = 180f;

        [Tooltip("Optional speed curve over time to control rotation speed dynamically.")]
        public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Tooltip("Loop duration in seconds for the speed curve.")]
        public float curveDuration = 2f;

        private float elapsedTime = 0f;

        void Update()
        {
            // Update elapsed time and loop it according to curveDuration
            elapsedTime += Time.deltaTime;
            if (elapsedTime > curveDuration)
                elapsedTime -= curveDuration;

            // Evaluate speed multiplier from the animation curve (value between 0 and 1 usually)
            float speedMultiplier = speedCurve.Evaluate(elapsedTime / curveDuration);

            // Calculate rotation amount for this frame
            float rotationAmount = baseSpinSpeed * speedMultiplier * Time.deltaTime;

            // Rotate around Y axis
            transform.Rotate(0f, rotationAmount, 0f);
        }
    }
}
