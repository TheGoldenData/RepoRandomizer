using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using REPORandomizer.Core;
using REPORandomizer.Effects;
using System.Collections.Generic;
using UnityEngine;

namespace REPORandomizer
{
    public class RandomizerManager : IOnEventCallback
    {
        private const byte EVENT_TRIGGER_EFFECT = 50;
        private const byte EVENT_DEACTIVATE_EFFECT = 51;
        private const byte EVENT_TIMER_SYNC = 52;
        private const byte EVENT_CONFIG_SYNC = 53;

        private readonly EffectRegistry registry;
        private IRandomEffect currentEffect;
        private float timer;
        private float interval => ModConfig.GetInterval(); // 30s? | 60s? | 90s? | 120s?
        private float duration;
        private bool callbackRegistered = false;

        private float clientTimeUntilNext;

        private float heartbeatTimer;
        private const float HEARTBEAT_INTERVAL = 2f;

        public RandomizerManager()
        {
            var registry = new EffectRegistry();

            registry.RegisterEffect(new SwappedControlsEffect(ModConfig.GetDuration("swapped_controls")));
            registry.RegisterEffect(new DrunkPlayerEffect(ModConfig.GetDuration("drunk")));
            registry.RegisterEffect(new NoUIEffect(ModConfig.GetDuration("no_ui")));
            registry.RegisterEffect(new SlowMotionEffect(ModConfig.GetDuration("slow_motion")));
            registry.RegisterEffect(new FastMotionEffect(ModConfig.GetDuration("fast_motion")));
            registry.RegisterEffect(new FastEnemiesEffect(ModConfig.GetDuration("fast_enemies")));
            registry.RegisterEffect(new NoStrengthEffect(ModConfig.GetDuration("no_strength")));
            registry.RegisterEffect(new GrantWingsEffect(ModConfig.GetDuration("grant_wings")));
            registry.RegisterEffect(new DeafPlayerEffect(ModConfig.GetDuration("deaf_player")));
            registry.RegisterEffect(new DisabledMapEffect(ModConfig.GetDuration("disabled_map")));
            registry.RegisterEffect(new FlickeringFlashlightEffect(ModConfig.GetDuration("flickering_flashlight")));
            registry.RegisterEffect(new NoSprintEffect(ModConfig.GetDuration("no_sprint")));
            registry.RegisterEffect(new StuckGrabberEffect(ModConfig.GetDuration("stuck_grabber")));

            this.registry = registry;
        }

        private void EnsureCallbackRegistered()
        {
            if (callbackRegistered) return;
            PhotonNetwork.AddCallbackTarget(this);
            callbackRegistered = true;
        }

        public void Update(float time)
        {
            EnsureCallbackRegistered();

            if (currentEffect != null && currentEffect.IsActive)
            {
                duration += time;
                currentEffect.Update(time);

                if (SemiFunc.IsMasterClientOrSingleplayer() && duration >= currentEffect.Duration)
                {
                    BroadcastDeactivate();
                    DeactivateCurrentEffectLocal();
                }
            }

            if (!SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (clientTimeUntilNext > 0f)
                {
                    clientTimeUntilNext -= time;
                    if (clientTimeUntilNext < 0f) clientTimeUntilNext = 0f;
                }
                return;
            }

            timer += time;
            if (timer >= interval)
            {
                timer = 0;
                TriggerNextEffect();
            }

            heartbeatTimer += time;
            if (heartbeatTimer >= HEARTBEAT_INTERVAL)
            {
                heartbeatTimer = 0f;
                BroadcastTimerSync();
            }
        }

        private void TriggerNextEffect()
        {
            if (currentEffect != null)
            {
                BroadcastDeactivate();
                DeactivateCurrentEffectLocal();
            }

            var nextEffect = registry.GetRandomEffect();
            if (nextEffect == null) return;

            BroadcastTrigger(nextEffect.Id.Value, interval);
            ApplyTriggerLocal(nextEffect.Id.Value);
        }

        private void BroadcastTrigger(string effectId, float effectDuration)
        {
            if (!SemiFunc.IsMultiplayer()) return;

            object[] content = new object[] { effectId, effectDuration, interval };
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };

            PhotonNetwork.RaiseEvent(EVENT_TRIGGER_EFFECT, content, options, sendOptions);
            Debug.Log($"[RandomizerMod] Broadcast trigger: {effectId}");
        }

        private void BroadcastDeactivate()
        {
            if (!SemiFunc.IsMultiplayer()) return;

            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };

            PhotonNetwork.RaiseEvent(EVENT_DEACTIVATE_EFFECT, null, options, sendOptions);
            Debug.Log("[RandomizerMod] Broadcast deactivate");
        }

        private void BroadcastTimerSync()
        {
            if (!SemiFunc.IsMultiplayer()) return;

            float timeLeft = Mathf.Max(0f, interval - timer);
            object[] content = new object[] { timeLeft };
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = false };

            PhotonNetwork.RaiseEvent(EVENT_TIMER_SYNC, content, options, sendOptions);
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case EVENT_TRIGGER_EFFECT:
                    object[] data = (object[])photonEvent.CustomData;
                    string effectId = (string)data[0];
                    float hostEffectDuration = (float)data[1];

                    ApplyTriggerLocal(effectId);

                    // Nadpisz Duration lokalnej instancji efektu wartością hosta
                    if (currentEffect != null)
                    {
                        currentEffect.OverrideDuration(hostEffectDuration);
                    }

                    try { clientTimeUntilNext = (float)data[2]; }
                    catch { }
                    break;

                case EVENT_DEACTIVATE_EFFECT:
                    DeactivateCurrentEffectLocal();
                    break;

                case EVENT_TIMER_SYNC:
                    object[] syncData = (object[])photonEvent.CustomData;
                    clientTimeUntilNext = (float)syncData[0];
                    break;

                case EVENT_CONFIG_SYNC:
                    object[] configData = (object[])photonEvent.CustomData;
                    float remoteInterval = (float)configData[0];
                    string[] keys = (string[])configData[1];
                    float[] values = (float[])configData[2];

                    var remoteDurations = new Dictionary<string, float>();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        remoteDurations[keys[i]] = values[i];
                    }

                    ModConfig.ApplyRemoteConfig(remoteInterval, remoteDurations);
                    break;
            }
        }

        private void ApplyTriggerLocal(string effectId)
        {
            currentEffect = registry.GetEffectById(effectId);
            duration = 0f;
            if (currentEffect == null) return;
            currentEffect.Trigger();
        }

        private void DeactivateCurrentEffectLocal()
        {
            if (currentEffect == null) return;
            currentEffect.Deactivate();
            currentEffect = null;
        }

        public void DeactivateCurrentEffect()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                BroadcastDeactivate();
            }
            DeactivateCurrentEffectLocal();
        }

        public void Dispose()
        {
            if (callbackRegistered)
            {
                PhotonNetwork.RemoveCallbackTarget(this);
                callbackRegistered = false;
            }
        }

        public IRandomEffect CurrentEffect => currentEffect;
        public EffectType CurrentEffectType => currentEffect != null ? currentEffect.Type : EffectType.Neutral;
        public float CurrentEffectDuration => duration;
        public float TimeUntilNextEffect
        {
            get
            {
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    return Mathf.Max(0f, interval - timer);
                }
                return clientTimeUntilNext;
            }
        }
        public int GetRegistryCount => registry.Count;
    }
}