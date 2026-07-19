using HarmonyLib;

namespace REPORandomizer.Patches
{
    [HarmonyPatch(typeof(InputManager))]
    public static class PlayerMovementPatch
    {
        public static bool ControlSwapped = false;

        [HarmonyPatch("GetMovementX")]
        [HarmonyPostfix]
        public static void PostfixX(ref float __result)
        {
            if (!ControlSwapped) return;
            __result = -__result;
        }

        [HarmonyPatch("GetMovementY")]
        [HarmonyPostfix]
        public static void PostfixY(ref float __result)
        {
            if (!ControlSwapped) return;
            __result = -__result;
        }
    }
}
