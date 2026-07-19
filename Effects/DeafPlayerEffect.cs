using REPORandomizer.Core;
using UnityEngine;

namespace REPORandomizer.Effects
{
    public class DeafPlayerEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("deaf_player");
        public string Name => Localization.Get("effect.deaf_player.name");
        public string Description => Localization.Get("effect.deaf_player.description", Duration);
        public EffectType Type => EffectType.Negative;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public DeafPlayerEffect(float duration) => Duration = duration;

        private float originalVolume;

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            AudioListener.volume = originalVolume;
        }

        public void Trigger()
        {
            IsActive = true;
            originalVolume = AudioListener.volume;
            AudioListener.volume = 0f;
        }

        public void Update(float time) { }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}
