using HarmonyLib;
using REPORandomizer.Effects;

namespace REPORandomizer.Patches
{
    [HarmonyPatch(typeof(MapToolController), "Update")]
    public static class DisabledMapPatch
    {
        public static void Postfix(MapToolController __instance)
        {
            if (__instance == null) return;
            if (__instance.DisplayMesh)
            {
                __instance.DisplayMesh.enabled = !DisabledMapEffect.active;
            }
        }
    }
}
