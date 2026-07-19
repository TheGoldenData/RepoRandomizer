using HarmonyLib;
using REPORandomizer.Effects;

namespace REPORandomizer.Patches
{
    [HarmonyPatch(typeof(PlayerController), "FixedUpdate")]
    public static class NoSprintPatch
    {
        private static readonly AccessTools.FieldRef<PlayerController, bool> toggleSprintRef = AccessTools.FieldRefAccess<PlayerController, bool>("toggleSprint");

        public static void Postfix(PlayerController __instance)
        {
            if (!NoSprintEffect.active) return;

            __instance.sprinting = false;
            __instance.SprintSpeedLerp = 0f;
            toggleSprintRef(__instance) = false;
        }
    }
}
