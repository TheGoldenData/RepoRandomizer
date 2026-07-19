using REPORandomizer.Core;
using System.Collections.Generic;
using UnityEngine;

namespace REPORandomizer.Effects
{
    public class NoStrengthEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("no_strength");
        public string Name => Localization.Get("effect.no_strength.name");
        public string Description => Localization.Get("effect.no_strength.description", Duration);
        public EffectType Type => EffectType.Negative;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public NoStrengthEffect(float duration) => Duration = duration;

        private Dictionary<string, int> originalValues = new Dictionary<string, int>();
        private Dictionary<string, PhysGrabber> grabbers = new Dictionary<string, PhysGrabber>();

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;

            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            foreach (var kvp in originalValues)
            {
                string steamID = kvp.Key;
                int originalStrength = kvp.Value;

                StatsManager.instance.playerUpgradeStrength.TryGetValue(steamID, out int boughtDuringEffect);
                int restored = originalStrength + boughtDuringEffect;

                PunManager.instance.UpdateStat("playerUpgradeStrength", steamID, restored);

                if (grabbers.TryGetValue(steamID, out PhysGrabber grabber)
                    && (UnityEngine.Object)grabber != null)
                {
                    grabber.grabStrength = 1f + restored * 0.2f;
                    Debug.Log($"[NoStrengthEffect] {steamID}: grabStrength = {grabber.grabStrength} ({restored} upgrades)");
                }
            }

            originalValues.Clear();
            grabbers.Clear();
        }
        public void Trigger()
        {
            IsActive = true;
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            originalValues.Clear();
            grabbers.Clear();

            foreach (var playerAvatar in GameDirector.instance.PlayerList)
            {
                string steamID = SemiFunc.PlayerGetSteamID(playerAvatar);
                if (string.IsNullOrEmpty(steamID)) continue;

                StatsManager.instance.playerUpgradeStrength.TryGetValue(steamID, out int originalValue);
                originalValues[steamID] = originalValue;
                PunManager.instance.UpdateStat("playerUpgradeStrength", steamID, 0);

                var grabber = playerAvatar.GetComponentInChildren<PhysGrabber>();
                if (grabber == null) continue;

                grabbers[steamID] = grabber;
                grabber.grabStrength = 1f;

                Debug.Log($"[NoStrengthEffect] {steamID}: upgrades={originalValue}, grabStrength = 1");
            }
        }

        public void Update(float time) { }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}