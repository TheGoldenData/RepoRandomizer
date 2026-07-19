using REPORandomizer.Core;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace REPORandomizer.Effects
{
    public class FastEnemiesEffect : IRandomEffect
    {
        public EffectId Id => EffectId.Of("fast_enemies");
        public string Name => Localization.Get("effect.fast_enemies.name");
        public string Description => Localization.Get("effect.fast_enemies.description", Duration);
        public EffectType Type => EffectType.Negative;
        public bool IsActive { get; private set; }
        public float Duration { get; private set; }
        public FastEnemiesEffect(float duration) => Duration = duration;

        private const float SpeedMultiplier = 3f;
        private Dictionary<MonoBehaviour, (float speed, float accel)> originalValues = new Dictionary<MonoBehaviour, (float speed, float accel)>();

        private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static FieldInfo _spawnedField;
        private static FieldInfo _enemyField;
        private static FieldInfo _hasNavField;
        private static FieldInfo _navWrapperField;
        private static FieldInfo _speedField;
        private static FieldInfo _accelField;
        private static MethodInfo _updateAgentMethod;
        private static bool _reflectionCached = false;

        public void Trigger()
        {
            IsActive = true;

            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            originalValues.Clear();
            ApplyToAllCurrentEnemies();
        }

        public void Update(float time)
        {
            if (!IsActive) return;
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            ReapplyAll();
            ApplyToAllCurrentEnemies();
        }

        private static void EnsureReflectionCached(MonoBehaviour enemyParentSample)
        {
            if (_reflectionCached) return;

            var enemyParentType = enemyParentSample.GetType();
            _spawnedField = enemyParentType.GetField("Spawned", Flags);
            _enemyField = enemyParentType.GetField("Enemy", Flags);

            if (_spawnedField is null || _enemyField is null)
            {
                Debug.LogWarning("[RandomizerMod] FastEnemiesEffect: not found fields Spawned/Enemy on EnemyParent.");
                return;
            }

            var enemyObj = _enemyField.GetValue(enemyParentSample) as MonoBehaviour;
            UnityEngine.Object enemyAsUnityObj = enemyObj;
            if (enemyAsUnityObj == null) return;

            var enemyType = enemyObj.GetType();
            _hasNavField = enemyType.GetField("HasNavMeshAgent", Flags);
            _navWrapperField = enemyType.GetField("NavMeshAgent", Flags);

            if (_hasNavField is null || _navWrapperField is null)
            {
                Debug.LogWarning("[RandomizerMod] FastEnemiesEffect:  not found fields HasNavMeshAgent/NavMeshAgent in Enemy!");
                return;
            }

            bool hasNav = (bool)_hasNavField.GetValue(enemyObj);
            if (!hasNav) return;

            var wrapperObj = _navWrapperField.GetValue(enemyObj) as MonoBehaviour;
            UnityEngine.Object wrapperAsUnityObj = wrapperObj;
            if (wrapperAsUnityObj == null) return;

            var wrapperType = wrapperObj.GetType();
            _speedField = wrapperType.GetField("DefaultSpeed", Flags);
            _accelField = wrapperType.GetField("DefaultAcceleration", Flags);
            _updateAgentMethod = wrapperType.GetMethod("UpdateAgent", Flags);

            if (_speedField is null || _accelField is null || _updateAgentMethod is null)
            {
                Debug.LogWarning("[RandomizerMod] FastEnemiesEffect: not found DefaultSpeed/DefaultAcceleration/UpdateAgent in EnemyNavMeshAgent!");
                return;
            }

            _reflectionCached = true;
            Debug.Log("[RandomizerMod] FastEnemiesEffect: reflections works correctly.");
        }

        private void ApplyToAllCurrentEnemies()
        {
            var enemyParents = EnemyDirector.instance?.enemiesSpawned;
            if (enemyParents is null) return;

            foreach (var enemyParent in enemyParents)
            {
                UnityEngine.Object enemyParentAsUnityObj = enemyParent as UnityEngine.Object;
                if (enemyParentAsUnityObj == null) continue;

                var enemyParentMb = enemyParent as MonoBehaviour;
                if (enemyParentMb is null) continue;

                if (!_reflectionCached)
                {
                    EnsureReflectionCached(enemyParentMb);
                    if (!_reflectionCached) continue;
                }

                bool spawned = (bool)_spawnedField.GetValue(enemyParentMb);
                if (!spawned) continue;

                var enemyObj = _enemyField.GetValue(enemyParentMb) as MonoBehaviour;
                UnityEngine.Object enemyAsUnityObj = enemyObj;
                if (enemyAsUnityObj == null) continue;

                bool hasNav = (bool)_hasNavField.GetValue(enemyObj);
                if (!hasNav) continue;

                var wrapperObj = _navWrapperField.GetValue(enemyObj) as MonoBehaviour;
                UnityEngine.Object wrapperAsUnityObj = wrapperObj;
                if (wrapperAsUnityObj == null) continue;

                if (originalValues.ContainsKey(wrapperObj)) continue;

                float baseSpeed = (float)_speedField.GetValue(wrapperObj);
                float baseAccel = (float)_accelField.GetValue(wrapperObj);

                originalValues[wrapperObj] = (baseSpeed, baseAccel);

                CallUpdateAgent(wrapperObj, baseSpeed * SpeedMultiplier, baseAccel);
            }
        }

        private void ReapplyAll()
        {
            if (!_reflectionCached) return;

            List<MonoBehaviour> deadKeys = null;

            foreach (var kvp in originalValues)
            {
                UnityEngine.Object wrapperAsUnityObj = kvp.Key;
                if (wrapperAsUnityObj == null)
                {
                    if (deadKeys == null) deadKeys = new List<MonoBehaviour>();
                    deadKeys.Add(kvp.Key);
                    continue;
                }

                CallUpdateAgent(kvp.Key, kvp.Value.speed * SpeedMultiplier, kvp.Value.accel);
            }

            if (deadKeys != null)
            {
                foreach (var key in deadKeys)
                {
                    originalValues.Remove(key);
                }
            }
        }

        private void CallUpdateAgent(MonoBehaviour wrapper, float speed, float accel)
        {
            _updateAgentMethod?.Invoke(wrapper, new object[] { speed, accel });
        }

        public void Deactivate()
        {
            IsActive = false;

            if (!SemiFunc.IsMasterClientOrSingleplayer()) { originalValues.Clear(); return; }
            if (!_reflectionCached) { originalValues.Clear(); return; }

            int restoredCount = 0;
            foreach (var kvp in originalValues)
            {
                UnityEngine.Object wrapperAsUnityObj = kvp.Key;
                if (wrapperAsUnityObj == null) continue;

                CallUpdateAgent(kvp.Key, kvp.Value.speed, kvp.Value.accel);
                restoredCount++;
            }
            originalValues.Clear();
            Debug.Log($"[RandomizerMod] FastEnemiesEffect: przywrócono prędkość dla {restoredCount} wrogów.");
        }

        public void OverrideDuration(float newDuration)
        {
            Duration = newDuration;
        }
    }
}