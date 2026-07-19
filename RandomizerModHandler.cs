using REPORandomizer;
using REPORandomizer.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomizerModHandler : MonoBehaviour
{
    public RandomizerManager Manager { get; private set; }
    private bool levelInitialized = false;
    private object levelGeneratorInstance = null;
    private Type levelGeneratorType = null;
    private bool isInPlayeableLevel = false;

    public void Initialize(RandomizerManager manager)
    {
        Manager = manager;
        SceneManager.sceneLoaded += OnSceneLoaded;
        IsPlayeableLevel();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsPlayeableLevel();
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void IsPlayeableLevel()
    {
        if (!SemiFunc.RunIsLevel())
        {
            isInPlayeableLevel = false;
            levelInitialized = false;
            Manager?.DeactivateCurrentEffect();
            DeathTracker.Reset();
        }
        else
        {
            isInPlayeableLevel = true;
        }
    }
    void Update()
    {
        if (Manager is null || !isInPlayeableLevel) return;

        if (!levelInitialized)
        {
            if (TryGetLevelGeneratorInstance(out levelGeneratorInstance, out levelGeneratorType))
            {
                var generatedField = levelGeneratorType.GetField("Generated",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (generatedField != null && (bool)generatedField.GetValue(levelGeneratorInstance) == true)
                {
                    levelInitialized = true;
                    Debug.Log("[RandomizerMod] Level is initialized");
                }
            }
            else
            {
                Debug.Log("[RandomizerMod] LevelGenerator instance not found!");
            }
            return;
        }
        Manager?.Update(Time.deltaTime);
    }
    private bool TryGetLevelGeneratorInstance(out object instance, out Type type)
    {
        type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == "LevelGenerator");

        if (type != null)
        {
            var instanceField = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceField != null)
            {
                instance = instanceField.GetValue(null);
                return instance != null;
            }
        }

        instance = null;
        return false;
    }

    void OnGUI()
    {
        if (Manager is null) return;

        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.alignment = TextAnchor.UpperRight;
        textStyle.fontSize = 13;
        textStyle.normal.textColor = Color.white;
        textStyle.wordWrap = false;

        GUIStyle boldStyle = new GUIStyle(textStyle);
        boldStyle.fontStyle = FontStyle.Bold;

        string activeEffectName = Localization.Get("ui.no_effect");
        string activeEffectDesc = Localization.Get("ui.no_effect_desc");
        Color activeEffectColor = Color.white;

        if (Manager.CurrentEffect != null && Manager.CurrentEffect.IsActive)
        {
            float timeLeft = Mathf.Max(0, Manager.CurrentEffect.Duration - Manager.CurrentEffectDuration);
            activeEffectName = $"{Manager.CurrentEffect.Name} ({timeLeft:F1}s)";
            activeEffectDesc = Manager.CurrentEffect.Description;
            activeEffectColor = EffectTypeColors.GetColor(Manager.CurrentEffect.Type);
        }

        string activeLabel = Localization.Get("ui.active_effect");
        string nextLabel = Localization.Get("ui.next_in");

        float effectWidth = Mathf.Max(
            boldStyle.CalcSize(new GUIContent($"{activeLabel}: {activeEffectName}")).x,
            textStyle.CalcSize(new GUIContent(activeEffectDesc)).x,
            boldStyle.CalcSize(new GUIContent($"{nextLabel}: {Manager.TimeUntilNextEffect:F1}s")).x
        ) + 20f;

        float effectHeight = 90f;
        float effectPosX = Screen.width - effectWidth - 400f;
        float effectPosY = 20f;

        GUI.Box(new Rect(effectPosX, effectPosY, effectWidth, effectHeight), "");

        string hexColor = ColorUtility.ToHtmlStringRGBA(activeEffectColor);
        GUI.Label(new Rect(effectPosX - 10f, effectPosY + 5f, effectWidth, 20f), $"{activeLabel}: <color=#{hexColor}>{activeEffectName}</color>", boldStyle);
        GUI.Label(new Rect(effectPosX - 10f, effectPosY + 25f, effectWidth, 40f), activeEffectDesc, textStyle);
        GUI.Label(new Rect(effectPosX - 10f, effectPosY + 65f, effectWidth, 20f), $"{nextLabel}: {Manager.TimeUntilNextEffect:F1}s", boldStyle);

        var playerList = GameDirector.instance?.PlayerList;
        if (playerList == null || playerList.Count == 0) return;

        string deathsHeader = Localization.Get("ui.deaths_header");
        float rowHeight = 20f;
        float maxDeathWidth = boldStyle.CalcSize(new GUIContent(deathsHeader)).x;

        for (int i = 0; i < playerList.Count; i++)
        {
            var player = playerList[i];
            if (player == null) continue;

            string name = SemiFunc.PlayerGetName(player);
            DeathTracker.GetDeaths().TryGetValue(SemiFunc.PlayerGetSteamID(player), out int deaths);

            float rowWidth = textStyle.CalcSize(new GUIContent($"{name}: {deaths}")).x;
            if (rowWidth > maxDeathWidth) maxDeathWidth = rowWidth;
        }

        float deathWidth = maxDeathWidth + 20f;
        float deathHeight = rowHeight + playerList.Count * rowHeight + 10f;
        float deathPosX = effectPosX - deathWidth - 10f;
        float deathPosY = effectPosY;

        GUI.Box(new Rect(deathPosX, deathPosY, deathWidth, deathHeight), "");
        GUI.Label(new Rect(deathPosX - 10f, deathPosY + 5f, deathWidth, rowHeight), deathsHeader, boldStyle);

        for (int i = 0; i < playerList.Count; i++)
        {
            var player = playerList[i];
            if (player == null) continue;

            string name = SemiFunc.PlayerGetName(player);
            DeathTracker.GetDeaths().TryGetValue(SemiFunc.PlayerGetSteamID(player), out int deaths);

            GUI.Label(new Rect(deathPosX - 10f, deathPosY + 5f + rowHeight + i * rowHeight, deathWidth, rowHeight), $"{name}: {deaths}", textStyle);
        }
    }
}
