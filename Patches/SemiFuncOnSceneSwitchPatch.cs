using HarmonyLib;

namespace REPORandomizer.Patches
{
    [HarmonyPatch(typeof(SemiFunc), "OnSceneSwitch")]
    public static class SemiFuncOnSceneSwitchPatch
    {
        public static void Prefix()
        {
            RepoRandomizer.Manager?.DeactivateCurrentEffect();
            UnityEngine.Debug.Log("[REPO Randomizer] Harmony patch - SemiFuncOnSceneSwitchPatch");
        }
    }
}
