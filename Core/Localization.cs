using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace REPORandomizer.Core
{
    public static class Localization
    {
        private static Dictionary<string, string> strings = new Dictionary<string, string>();
        public static string CurrentLanguage { get; private set; } = "en";

        public static void Load(string languageCode)
        {
            CurrentLanguage = languageCode;

            string dllPath = Assembly.GetExecutingAssembly().Location;
            string dllDirectory = Path.GetDirectoryName(dllPath);
            string path = Path.Combine(dllDirectory, $"{languageCode}.json");

            if (!File.Exists(path))
            {
                path = Path.Combine(dllDirectory, "en.json");
            }

            if (!File.Exists(path)) return;

            string json = File.ReadAllText(path);
            strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }

        public static string Get(string key, params object[] args)
        {
            if (strings.TryGetValue(key, out string value))
            {
                return args.Length > 0 ? string.Format(value, args) : value;
            }
            return key;
        }
    }
}
