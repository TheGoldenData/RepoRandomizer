using REPORandomizer.Core;

namespace REPORandomizer.Effects
{
    public class StuckGrabberEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("stuck_grabber");
        public string Name => Localization.Get("effect.stuck_grabber.name");
        public string Description => Localization.Get("effect.stuck_grabber.description", Duration);
        public EffectType Type => EffectType.Negative;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public StuckGrabberEffect(float duration) => Duration = duration;

        public static bool active = false;
        public static bool allowNextRelease = false;

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            active = false;
            allowNextRelease = false;
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