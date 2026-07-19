using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace REPORandomizer.Patches
{
    public static class VoicePitchSync
    {
        private static Harmony harmony;
        private static int refCount = 0;

        private static readonly System.Reflection.MethodInfo awakeMethod = AccessTools.Method(typeof(PlayerVoiceChat), "Awake");

        public static void Acquire()
        {
            refCount++;
            if (harmony != null) return;

            harmony = new Harmony($"{RepoRandomizer.M_GUID}.voicepitchsync");

            var postfix = new HarmonyMethod(typeof(VoicePitchSync), nameof(OnVoiceChatAwake));
            harmony.Patch(awakeMethod, postfix: postfix);

            foreach (var vc in Object.FindObjectsOfType<PlayerVoiceChat>())
            {
                EnsureRelay(vc);
            }
        }

        public static void Release()
        {
            if (refCount > 0) refCount--;
            if (refCount > 0) return;

            harmony?.UnpatchSelf();
            harmony = null;
        }

        private static void OnVoiceChatAwake(PlayerVoiceChat __instance)
        {
            EnsureRelay(__instance);
        }

        private static void EnsureRelay(PlayerVoiceChat vc)
        {
            if (vc == null) return;
            if (vc.GetComponent<VoicePitchRelay>() == null)
                vc.gameObject.AddComponent<VoicePitchRelay>();
        }

        public static void SetPitch(float multiplier, float timeIn, float timeOut,
            float overrideTimer = 0.1f, float oscillation = 0f, float oscillationSpeed = 0f)
        {
            PlayerVoiceChat vc = PlayerVoiceChat.instance;
            if (vc == null) return;

            if (!SemiFunc.IsMultiplayer())
            {
                vc.OverridePitch(multiplier, timeIn, timeOut, overrideTimer, oscillation, oscillationSpeed);
                return;
            }

            PhotonView pv = vc.GetComponent<PhotonView>();
            if (pv == null) return;

            pv.RPC("OverridePitchRPC", RpcTarget.All,
                multiplier, timeIn, timeOut, overrideTimer, oscillation, oscillationSpeed);
        }
    }
}