using Photon.Pun;
using UnityEngine;

namespace REPORandomizer.Patches
{
    public class VoicePitchRelay : MonoBehaviour
    {
        [PunRPC]
        public void OverridePitchRPC(float multiplier, float timeIn, float timeOut,
            float overrideTimer, float oscillation, float oscillationSpeed)
        {
            PlayerVoiceChat vc = GetComponent<PlayerVoiceChat>();
            vc?.OverridePitch(multiplier, timeIn, timeOut, overrideTimer, oscillation, oscillationSpeed);
        }
    }
}