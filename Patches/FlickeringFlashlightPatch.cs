using HarmonyLib;
using REPORandomizer.Effects;

namespace REPORandomizer.Patches
{
    [HarmonyPatch(typeof(FlashlightController), "Update")]
    public static class FlickeringFlashlightPatch
    {
        public static void Postfix(FlashlightController __instance)
        {
            if (!FlickeringFlashlightEffect.active) return;
            if (__instance != FlashlightController.Instance) return;
            if (!__instance.LightActive) return;

            __instance.spotlight.enabled = FlickeringFlashlightEffect.CurrentlyOn;
        }
    }
}
