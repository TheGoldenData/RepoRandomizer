using REPORandomizer.Core;

namespace REPORandomizer.Effects
{
    public class DisabledMapEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("disabled_map");
        public string Name => Localization.Get("effect.disabled_map.name");
        public string Description => Localization.Get("effect.disabled_map.description", Duration);
        public EffectType Type => EffectType.Negative;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public DisabledMapEffect(float duration) => Duration = duration;

        public static bool active = false;

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
        }

        public void Update(float time) { }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}
