using BepInEx.Configuration;
using System.Collections.Generic;

namespace REPORandomizer.Core
{
    public static class ModConfig
    {
        public static ConfigEntry<string> Language { get; private set; }
        public static ConfigEntry<float> Interval { get; private set; }
        private static Dictionary<string, ConfigEntry<float>> effectDurations = new Dictionary<string, ConfigEntry<float>>();
        private static Dictionary<string, float> remoteDurations = null;
        private static float? remoteInterval = null;
        public static bool IsUsingRemoteConfig => remoteDurations != null;

        private static readonly Dictionary<string, float> defaultDurations = new Dictionary<string, float>
        {
            { "swapped_controls", 90f },
            { "drunk", 60f },
            { "no_ui", 90f },
            { "slow_motion", 22.5f },
            { "fast_motion", 60f },
            { "fast_enemies", 90f },
            { "no_strength", 90f },
            { "grant_wings", 30f },
            { "deaf_player", 60f },
            { "disabled_map", 60f },
            { "flickering_flashlight", 45f },
            { "no_sprint", 60f },
            { "stuck_grabber", 45f },
        };

        public static void Bind(ConfigFile config, ConfigEntry<string> languageEntry)
        {
            Language = languageEntry;

            Interval = config.Bind(
                "General", "Effect Interval", 90f,
                new ConfigDescription("Seconds between random effects", new AcceptableValueRange<float>(10f, 300f))
            );

            foreach (var kvp in defaultDurations)
            {
                string effectId = kvp.Key;

                string effectName = Localization.Get($"effect.{effectId}.name");

                var entry = config.Bind(
                    "Effects duration", effectName, kvp.Value,
                    new ConfigDescription($"Duration in seconds for effect '{effectId}'", new AcceptableValueRange<float>(10f, 300f))
                );
                effectDurations[effectId] = entry;
            }
        }

        public static float GetInterval()
        {
            return remoteInterval ?? Interval.Value;
        }

        public static float GetDuration(string effectId)
        {
            if (remoteDurations != null && remoteDurations.TryGetValue(effectId, out float remoteValue))
            {
                return UnityEngine.Mathf.Min(remoteValue, GetInterval());
            }

            if (effectDurations.TryGetValue(effectId, out var entry))
            {
                return UnityEngine.Mathf.Min(entry.Value, GetInterval());
            }

            return GetInterval();
        }

        public static Dictionary<string, float> GetAllLocalDurations()
        {
            var result = new Dictionary<string, float>();
            foreach (var kvp in effectDurations)
            {
                result[kvp.Key] = kvp.Value.Value;
            }
            return result;
        }

        public static void ApplyRemoteConfig(float hostInterval, Dictionary<string, float> hostDurations)
        {
            remoteInterval = hostInterval;
            remoteDurations = hostDurations;
        }

        public static void ClearRemoteConfig()
        {
            remoteInterval = null;
            remoteDurations = null;
        }
    }
}
