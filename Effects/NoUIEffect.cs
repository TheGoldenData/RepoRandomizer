using REPORandomizer.Core;

namespace REPORandomizer.Effects
{
    public class NoUIEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("no_ui");
        public string Name => Localization.Get("effect.no_ui.name");
        public string Description => Localization.Get("effect.no_ui.description", Duration);
        public EffectType Type => EffectType.Neutral;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public NoUIEffect(float duration) => Duration = duration;

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
        }

        public void Trigger()
        {
            IsActive = true;
        }

        public void Update(float time)
        {
            if (!IsActive) return;
            SemiFunc.UIHideHealth();
            SemiFunc.UIHideEnergy();
            SemiFunc.UIHideInventory();
            SemiFunc.UIHideHaul();
            SemiFunc.UIHideGoal();
        }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}
