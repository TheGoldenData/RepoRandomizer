using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using REPORandomizer;
using REPORandomizer.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

[BepInPlugin(M_GUID, M_NAME, M_VERSION)]
public class RepoRandomizer : BaseUnityPlugin
{
    public const string M_GUID = "thegoldendata.reporandomizer";
    const string M_NAME = "REPO Randomizer";
    const string M_VERSION = "1.0.0";
    public static RandomizerManager Manager { get; private set; }
    public static RepoRandomizer Instance { get; private set; }
    private void Awake()
    {
        Instance = this;

        var languageEntry = Config.Bind(
                "General", "Language", "en",
                new ConfigDescription("Language for effect names/descriptions", new AcceptableValueList<string>("en", "pl"))
            );

        Localization.Load(languageEntry.Value);

        ModConfig.Bind(Config, languageEntry);

        ModConfig.Language.SettingChanged += (s, e) => Localization.Load(ModConfig.Language.Value);

        Manager = new RandomizerManager();

        var harmony = new Harmony(M_GUID);
        harmony.PatchAll();
        SceneManager.sceneLoaded += OnSceneLoaded;

        Logger.LogInfo($"Plugin loaded and initilized. Effects registered: {Manager.GetRegistryCount}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Logger.LogInfo($"Scene loaded: {scene.name}");

        if (GameObject.Find("RandomizerModHandler") == null)
        {
            var gameObject = new GameObject("RandomizerModHandler");
            var handler = gameObject.AddComponent<RandomizerModHandler>();
            handler.Initialize(Manager);
            DontDestroyOnLoad(gameObject);
        }
    }


}