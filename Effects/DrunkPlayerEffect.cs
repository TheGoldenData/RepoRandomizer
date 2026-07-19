using REPORandomizer.Core;
using UnityEngine;

namespace REPORandomizer.Effects
{
    public class DrunkPlayerEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("drunk");
        public string Name => Localization.Get("effect.drunk.name");
        public string Description => Localization.Get("effect.drunk.description", Duration);
        public EffectType Type => EffectType.Neutral;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public DrunkPlayerEffect(float duration) => Duration = duration;


        public static bool active = false;
        public static float timer = 0f;

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            active = false;
        }

        public void Trigger()
        {
            IsActive = true;
            active = true;
            timer = 0f;
        }

        public void Update(float time)
        {
            if (!IsActive) return;
            if (Cursor.lockState != CursorLockMode.Locked) return;

            timer += time;

            float distortion = Mathf.Sin(timer * 0.6f) * 35f + Mathf.Sin(timer * 1.7f) * 15f;
            PostProcessing.Instance.LensDistortionOverride(distortion, 5f, 5f, 0.2f, null);
        }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}