using HarmonyLib;
using REPORandomizer.Effects;

namespace REPORandomizer.Patches
{
    [HarmonyPatch(typeof(ItemEquippable), "RPC_UpdateItemState")]
    public static class ItemEquippableUnequipMarkerPatch
    {
        public static void Prefix(int state)
        {
            if (state == 2)
            {
                StuckGrabberEffect.allowNextRelease = true;
            }
        }
    }

    [HarmonyPatch(typeof(PhysGrabber), "ReleaseObject")]
    public static class StuckGrabberReleasePatch
    {
        private static readonly AccessTools.FieldRef<PlayerAvatar, bool> deadSetRef = AccessTools.FieldRefAccess<PlayerAvatar, bool>("deadSet");

        public static bool Prefix(PhysGrabber __instance)
        {
            if (!StuckGrabberEffect.active) return true;
            if (__instance != PhysGrabber.instance) return true;

            if (__instance.playerAvatar != null && deadSetRef(__instance.playerAvatar)) return true;

            if (StuckGrabberEffect.allowNextRelease)
            {
                StuckGrabberEffect.allowNextRelease = false;
                return true;
            }

            if (__instance.grabbed)
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PhysGrabber), "StartGrabbingPhysObject")]
    public static class StuckGrabberStartGrabPatch
    {
        public static bool Prefix(PhysGrabber __instance)
        {
            if (!StuckGrabberEffect.active) return true;
            if (__instance != PhysGrabber.instance) return true;

            if (__instance.grabbed)
            {
                return false;
            }

            return true;
        }
    }
}