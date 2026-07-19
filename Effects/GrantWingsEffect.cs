using HarmonyLib;
using REPORandomizer.Core;
using REPORandomizer.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace REPORandomizer.Effects
{
    public class GrantWingsEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("grant_wings");
        public string Name => Localization.Get("effect.grant_wings.name");
        public string Description => Localization.Get("effect.grant_wings.description", Duration);
        public EffectType Type => EffectType.Positive;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public GrantWingsEffect(float duration) => Duration = duration;

        private float timer;

        private const float DriftForce = 8f;

        private List<PlayerAvatar> affectedPlayers = new List<PlayerAvatar>();
        private Dictionary<PlayerAvatar, bool> wasTumbling = new Dictionary<PlayerAvatar, bool>();

        private static readonly AccessTools.FieldRef<PlayerAvatar, bool> IsDisabledRef = AccessTools.FieldRefAccess<PlayerAvatar, bool>("isDisabled");
        private static readonly AccessTools.FieldRef<PlayerAvatar, PlayerTumble> TumbleRef = AccessTools.FieldRefAccess<PlayerAvatar, PlayerTumble>("tumble");
        private static readonly AccessTools.FieldRef<PlayerAvatar, Vector3> InputDirectionRawRef = AccessTools.FieldRefAccess<PlayerAvatar, Vector3>("InputDirectionRaw");
        private static readonly AccessTools.FieldRef<PlayerAvatar, PlayerLocalCamera> LocalCameraRef = AccessTools.FieldRefAccess<PlayerAvatar, PlayerLocalCamera>("localCamera");
        private static readonly AccessTools.FieldRef<PlayerTumble, bool> IsTumblingRef = AccessTools.FieldRefAccess<PlayerTumble, bool>("isTumbling");
        private static readonly AccessTools.FieldRef<PlayerTumble, PhysGrabObject> PhysGrabObjectRef = AccessTools.FieldRefAccess<PlayerTumble, PhysGrabObject>("physGrabObject");
        private static readonly AccessTools.FieldRef<PhysGrabObject, Rigidbody> RbRef = AccessTools.FieldRefAccess<PhysGrabObject, Rigidbody>("rb");

        public float TimerNormalized()
        {
            if (Duration <= 0f) return 0f;
            return Mathf.Clamp01(timer / Duration);
        }

        public void Trigger()
        {
            IsActive = true;
            VoicePitchSync.Acquire();
            timer = Duration;

            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            affectedPlayers.Clear();
            wasTumbling.Clear();

            var players = GameDirector.instance?.PlayerList;
            if (players == null || players.Count == 0) return;

            foreach (var p in players)
            {
                if (p == null || IsDisabledRef(p)) continue;

                var tumble = TumbleRef(p);
                if (tumble == null) continue;

                bool wasInTumble = IsTumblingRef(tumble);
                wasTumbling[p] = wasInTumble;

                if (!wasInTumble) tumble.TumbleRequest(true, false);
                tumble.TumbleOverrideTime(Duration);
                p.UpgradeTumbleWingsVisualsActive(true, false);

                affectedPlayers.Add(p);
            }
        }

        public void Update(float time)
        {
            if (!IsActive) return;

            timer -= time;
            if (timer < 0f) timer = 0f;

            PlayerAvatar avatar = PlayerAvatar.instance;
            if (avatar is null || PlayerVoiceChat.instance is null) return;

            VoicePitchSync.SetPitch(0.65f, 1f, 1f, 3f, 0.1f, 20f);

            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            foreach (var p in affectedPlayers)
            {
                if ((UnityEngine.Object)p == null) continue;

                var tumble = TumbleRef(p);
                if (tumble == null) continue;

                tumble.TumbleOverrideTime(0.5f);
                p.UpgradeTumbleWingsVisualsActive(true, false);

                var physGrabObj = PhysGrabObjectRef(tumble);
                if (physGrabObj == null) continue;

                physGrabObj.OverrideZeroGravity(0.1f);
                physGrabObj.OverrideDrag(1f, 0.1f);
                physGrabObj.OverrideAngularDrag(1.5f, 0.1f);

                var localCamera = LocalCameraRef(p);
                if (localCamera == null) continue;
                Transform cam = localCamera.GetOverrideTransform();

                var rb = RbRef(physGrabObj);
                if (rb == null) continue;

                Quaternion targetRotation = Quaternion.LookRotation(cam.forward, Vector3.up);
                rb.AddTorque(SemiFunc.PhysFollowRotation(rb.transform, targetRotation, rb, 20f), ForceMode.Impulse);

                Vector3 inputDir = InputDirectionRawRef(p);
                if (inputDir.sqrMagnitude < 0.0001f) continue;

                Vector3 dir = cam.forward * inputDir.z + cam.right * inputDir.x;
                if (dir.sqrMagnitude < 0.0001f) continue;
                if (dir.sqrMagnitude > 1f) dir.Normalize();

                rb.AddForce(dir * DriftForce * rb.mass, ForceMode.Force);
            }
        }

        public void Deactivate()
        {
            IsActive = false;
            VoicePitchSync.Release();
            if (!SemiFunc.IsMasterClientOrSingleplayer())
            {
                affectedPlayers.Clear();
                wasTumbling.Clear();
                return;
            }

            foreach (var p in affectedPlayers)
            {
                if ((UnityEngine.Object)p == null) continue;

                p.UpgradeTumbleWingsVisualsActive(false, false);

                var tumble = TumbleRef(p);
                if (tumble != null && wasTumbling.TryGetValue(p, out bool wasInTumble) && !wasInTumble
                    && IsTumblingRef(tumble))
                {
                    tumble.TumbleRequest(false, false);
                }
            }
            affectedPlayers.Clear();
            wasTumbling.Clear();
        }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}