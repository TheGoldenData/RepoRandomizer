using UnityEngine;

namespace REPORandomizer.Core
{
    public static class EffectTypeColors
    {
        public static Color GetColor(EffectType type)
        {
            switch (type)
            {
                case EffectType.Positive: return Color.green;
                case EffectType.Negative: return Color.red;
                case EffectType.Neutral: return Color.yellow;
                default: return Color.white;
            }
        }
    }
}
