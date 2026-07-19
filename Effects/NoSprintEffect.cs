using REPORandomizer.Core;

namespace REPORandomizer.Effects
{
    public class NoSprintEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("no_sprint");
        public string Name => Localization.Get("effect.no_sprint.name");
        public string Description => Localization.Get("effect.no_sprint.description", Duration);
        public EffectType Type => EffectType.Negative;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public NoSprintEffect(float duration) => Duration = duration;

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
