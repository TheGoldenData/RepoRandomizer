using REPORandomizer.Core;
using REPORandomizer.Patches;

namespace REPORandomizer.Effects
{
    public class SlowMotionEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("slow_motion");
        public string Name => Localization.Get("effect.slow_motion.name");
        public string Description => Localization.Get("effect.slow_motion.description", Duration);
        public EffectType Type => EffectType.Negative;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public SlowMotionEffect(float duration) => Duration = duration;

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            VoicePitchSync.Release();
        }

        public void Trigger()
        {
            IsActive = true;
            VoicePitchSync.Acquire();
        }

        public void Update(float time)
        {
            if (!IsActive) return;

            // Same as ValuableWizardTimeGlass in game files
            PlayerController.instance.OverrideSpeed(0.5f, 0.1f);
            PlayerController.instance.OverrideLookSpeed(0.5f, 2f, 1f, 0.1f);
            PlayerController.instance.OverrideAnimationSpeed(0.2f, 1f, 2f, 0.1f);
            PlayerController.instance.OverrideTimeScale(0.1f, 0.1f);

            PlayerAvatar avatar = PlayerAvatar.instance;
            if (avatar is null || PlayerVoiceChat.instance is null) return;

            VoicePitchSync.SetPitch(0.65f, 1f, 2f, 0.1f, 0f, 0f);

            avatar.OverridePupilSize(3f, 4, 1f, 1f, 5f, 0.5f, 0.1f);
            CameraZoom.Instance.OverrideZoomSet(50f, 0.1f, 0.5f, 1f, null, 0);
            PostProcessing.Instance.SaturationOverride(50f, 0.1f, 0.5f, 0.1f, null);
        }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}
