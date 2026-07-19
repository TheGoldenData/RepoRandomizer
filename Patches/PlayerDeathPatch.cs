using HarmonyLib;

namespace REPORandomizer.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar), "PlayerDeathRPC")]
    public static class PlayerDeathPatch
    {
        public static void Postfix(PlayerAvatar __instance)
        {
            DeathTracker.RegisterDeath(__instance);
        }
    }
}
