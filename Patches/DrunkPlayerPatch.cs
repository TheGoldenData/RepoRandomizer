using HarmonyLib;
using UnityEngine;

namespace REPORandomizer.Effects
{
    [HarmonyPatch(typeof(InputManager))]
    public static class DrunkPlayerPatch
    {
        [HarmonyPatch("GetMovementX")]
        [HarmonyPostfix]
        public static void PostfixX(ref float __result)
        {
            if (!DrunkPlayerEffect.active || Cursor.lockState != CursorLockMode.Locked) return;
            float wobble = Mathf.Sin(DrunkPlayerEffect.timer * 2f) * 0.6f
                         + Mathf.Sin(DrunkPlayerEffect.timer * 5.3f) * 0.25f;
            __result += wobble;
            __result = Mathf.Clamp(__result, -1f, 1f);
        }

        [HarmonyPatch("GetMovementY")]
        [HarmonyPostfix]
        public static void PostfixY(ref float __result)
        {
            if (!DrunkPlayerEffect.active || Cursor.lockState != CursorLockMode.Locked) return;
            float wobble = Mathf.Cos(DrunkPlayerEffect.timer * 1.5f) * 0.35f
                         + Mathf.Sin(DrunkPlayerEffect.timer * 3.7f) * 0.15f;
            __result += wobble;
            __result = Mathf.Clamp(__result, -1f, 1f);
        }
    }
}