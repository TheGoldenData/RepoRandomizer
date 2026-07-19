using REPORandomizer.Core;
using REPORandomizer.Patches;

namespace REPORandomizer.Effects
{
    public class FastMotionEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("fast_motion");
        public string Name => Localization.Get("effect.fast_motion.name");
        public string Description => Localization.Get("effect.fast_motion.description", Duration);
        public EffectType Type => EffectType.Positive;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public FastMotionEffect(float duration) => Duration = duration;

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

            // Similar to SlowMotionEffect
            PlayerController.instance.OverrideSpeed(2f, 0.1f);
            PlayerController.instance.OverrideLookSpeed(100f, 0.2f, 0.5f, Duration);
            PlayerController.instance.OverrideAnimationSpeed(2f, 1f, 2f, 0.1f);

            PlayerAvatar avatar = PlayerAvatar.instance;
            if (avatar is null || PlayerVoiceChat.instance is null) return;

            VoicePitchSync.SetPitch(1.20f, 1f, 2f, 0.1f, 0f, 0f);

            avatar.OverridePupilSize(2f, 5, 5f, 0.5f, 5f, 0.5f, 0.1f);
            CameraZoom.Instance.OverrideZoomSet(70f, 0.1f, 0.5f, 1f, null, 0);
            PostProcessing.Instance.SaturationOverride(-20f, 0.1f, 0.5f, 0.1f, null);
        }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}
