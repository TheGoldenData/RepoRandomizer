using REPORandomizer.Core;
using REPORandomizer.Patches;

namespace REPORandomizer.Effects
{
    public class SwappedControlsEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("swapped_controls");
        public string Name => Localization.Get("effect.swapped_controls.name");
        public string Description => Localization.Get("effect.swapped_controls.description", Duration);
        public EffectType Type => EffectType.Negative;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public SwappedControlsEffect(float duration) => Duration = duration;

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            PlayerMovementPatch.ControlSwapped = false;
        }

        public void Trigger()
        {
            IsActive = true;
            PlayerMovementPatch.ControlSwapped = true;
        }

        public void Update(float time) { }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}