using REPORandomizer.Core;
using UnityEngine;

namespace REPORandomizer.Effects
{
    public class FlickeringFlashlightEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("flickering_flashlight");
        public string Name => Localization.Get("effect.flickering_flashlight.name");
        public string Description => Localization.Get("effect.flickering_flashlight.description", Duration);
        public EffectType Type => EffectType.Negative;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public FlickeringFlashlightEffect(float duration) => Duration = duration;

        public static bool active = false;
        public static bool CurrentlyOn = true;

        private const float MinInterval = 0.02f;
        private const float MaxInterval = 0.4f;

        private float flickerTimer;

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            active = false;
            CurrentlyOn = true;
        }

        public void Trigger()
        {
            IsActive = true;
            active = true;
            CurrentlyOn = true;
            flickerTimer = Random.Range(MinInterval, MaxInterval);
        }

        public void Update(float time)
        {
            if (!IsActive) return;

            flickerTimer -= time;
            if (flickerTimer <= 0f)
            {
                CurrentlyOn = !CurrentlyOn;
                flickerTimer = Random.Range(MinInterval, MaxInterval);
            }
        }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}