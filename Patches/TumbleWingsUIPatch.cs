using HarmonyLib;
using REPORandomizer.Effects;
using UnityEngine;

namespace REPORandomizer.Patches
{
    [HarmonyPatch(typeof(TumbleWingsUI), "Update")]
    public static class TumbleWingsUIPatch
    {
        private static readonly System.Reflection.MethodInfo showMethod = AccessTools.Method(typeof(SemiUI), "Show");
        private static readonly AccessTools.FieldRef<PlayerAvatar, bool> isTumblingRef = AccessTools.FieldRefAccess<PlayerAvatar, bool>("isTumbling");
        private static readonly AccessTools.FieldRef<PlayerAvatar, bool> isDisabledRef = AccessTools.FieldRefAccess<PlayerAvatar, bool>("isDisabled");

        public static void Postfix(TumbleWingsUI __instance)
        {
            var effect = RepoRandomizer.Manager?.CurrentEffect as GrantWingsEffect;
            if (effect == null || !effect.IsActive) return;
            if (!PlayerAvatar.instance) return;

            bool isTumbling = isTumblingRef(PlayerAvatar.instance);
            bool isDisabled = isDisabledRef(PlayerAvatar.instance);

            if (!isTumbling || isDisabled) return;

            showMethod?.Invoke(__instance, null);

            if (__instance.imageBar != null)
            {
                __instance.imageBar.rectTransform.localScale = new Vector3(effect.TimerNormalized(), 1f, 1f);
            }
        }
    }
}