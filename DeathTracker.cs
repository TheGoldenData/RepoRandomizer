using System.Collections.Generic;

namespace REPORandomizer
{
    public class DeathTracker
    {
        private static Dictionary<string, int> deathCounts = new Dictionary<string, int>();

        public static void RegisterDeath(PlayerAvatar avatar)
        {
            string steamID = SemiFunc.PlayerGetSteamID(avatar);
            if (string.IsNullOrEmpty(steamID)) return;

            if (!deathCounts.ContainsKey(steamID))
            {
                deathCounts[steamID] = 0;
            }

            deathCounts[steamID]++;
        }

        public static Dictionary<string, int> GetDeaths() => deathCounts;
        public static void Reset() => deathCounts.Clear();
    }
}
