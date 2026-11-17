using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Combat sound setup. AddToScene gives a scene's Player the CombatAudio component and its boss
// music; Add Combat Player To Open Scene calls it. Prepare() built the sound bank, the vampire's
// voice and the test scenes' audio once; it's no longer in the menu since all of that is
// committed. Lists are only filled while empty, so sounds swapped in the Inspector are kept.
public static class PrepareCombatAudio
{
    private const string Audio = "Assets/ThirdParty/Audio/";
    private const string BankPath = "Assets/Audio/CombatSounds.asset";
    private const string VampirePath = "Assets/Prefabs/Combat/Vampire.prefab";
    private const string MusicPath = Audio + "OpenGameArt/Music/EpicBossBattle.ogg";
    private const string VictoryPath = "Assets/Audio/VictorySting.ogg";
    private static readonly string[] TestScenes =
    {
        "Assets/Scenes/Archives/Tests/RangerMovementTest.unity",
        "Assets/Scenes/Archives/Tests/RangerMovementTestFemale.unity",
    };

    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        try
        {
            CombatSoundBank bank = GetOrCreateBank();
            SetUpVampireVoice();

            foreach (string path in TestScenes)
            {
                Scene scene = SceneManager.GetSceneByPath(path);
                bool wasOpen = scene.isLoaded;
                if (!wasOpen) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                AddToScene(scene);
                EditorSceneManager.SaveScene(scene);
                if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
            }

            Scene active = SceneManager.GetActiveScene();
            if (!TestScenes.Contains(active.path) && active.name != "Prototype" && AddToScene(active))
                EditorSceneManager.SaveScene(active);

            Selection.activeObject = bank;
            Debug.Log("Combat audio ready in the test scenes" + (active.name != "Prototype" ? " and " + active.name : "")
                + ". Swap sounds in " + BankPath + " and on the Vampire prefab's Enemy Voice.");
        }
        catch (Exception error)
        {
            Debug.LogError("Combat audio setup stopped: " + error.Message);
        }
    }

    // Adds CombatAudio to the scene's Player and music to its boss. Returns true if anything
    // was added. Also used by Add Combat Player To Open Scene.
    internal static bool AddToScene(Scene scene)
    {
        CombatSoundBank bank = AssetDatabase.LoadAssetAtPath<CombatSoundBank>(BankPath);
        if (bank == null) return false;
        bool changed = false;

        PlayerDefence player = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<PlayerDefence>(true)).FirstOrDefault();
        if (player != null)
        {
            CombatAudio audio = player.GetComponent<CombatAudio>();
            if (audio == null) { audio = player.gameObject.AddComponent<CombatAudio>(); changed = true; }
            if (audio.bank == null) { audio.bank = bank; EditorUtility.SetDirty(audio); changed = true; }
        }

        foreach (BossEncounter boss in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<BossEncounter>(true)))
        {
            if (boss.music == null) { boss.music = Load<AudioClip>(MusicPath); changed = true; }
            EditorUtility.SetDirty(boss);
        }

        if (changed) EditorSceneManager.MarkSceneDirty(scene);
        return changed;
    }

    private static CombatSoundBank GetOrCreateBank()
    {
        CombatSoundBank bank = AssetDatabase.LoadAssetAtPath<CombatSoundBank>(BankPath);
        if (bank == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Audio")) AssetDatabase.CreateFolder("Assets", "Audio");
            bank = ScriptableObject.CreateInstance<CombatSoundBank>();
            AssetDatabase.CreateAsset(bank, BankPath);
        }
        if (Empty(bank.swings)) bank.swings = Clips("OpenGameArt/Swishes", "swish-");
        if (Empty(bank.hits)) bank.hits = Clips("Kenney/Impacts", "impactPunch_medium");
        if (Empty(bank.heavyHits)) bank.heavyHits = Clips("Kenney/Impacts", "impactPunch_heavy");
        if (Empty(bank.blocks)) bank.blocks = Clips("Kenney/Impacts", "impactMetal_medium");
        if (Empty(bank.parries)) bank.parries = Clips("Kenney/Impacts", "impactBell_heavy");
        if (Empty(bank.rolls)) bank.rolls = Clips("Kenney/Foley", "cloth");
        if (bank.victory == null) bank.victory = Load<AudioClip>(VictoryPath);
        EditorUtility.SetDirty(bank);
        AssetDatabase.SaveAssets();
        return bank;
    }

    // Which monster sound goes where is a first guess by length: long roars to wake and die,
    // short ones for attacks and pain. Swap them on the prefab once you've heard them.
    private static void SetUpVampireVoice()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(VampirePath) == null) return;
        GameObject contents = PrefabUtility.LoadPrefabContents(VampirePath);
        try
        {
            EnemyVoice voice = contents.GetComponent<EnemyVoice>();
            if (voice == null) voice = contents.AddComponent<EnemyVoice>();
            if (Empty(voice.wake)) voice.wake = Monster(1, 2);
            if (Empty(voice.attack)) voice.attack = Monster(5, 6, 8, 10, 16, 17);
            if (Empty(voice.pain)) voice.pain = Monster(4, 7, 9, 18);
            if (Empty(voice.death)) voice.death = Monster(3, 14);
            if (Empty(voice.secondPhase)) voice.secondPhase = Monster(11, 15);
            PrefabUtility.SaveAsPrefabAsset(contents, VampirePath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private static AudioClip[] Monster(params int[] numbers) =>
        numbers.Select(n => Load<AudioClip>(Audio + "OpenGameArt/Monster/Monster-" + n + ".wav")).ToArray();

    private static AudioClip[] Clips(string folder, string prefix) =>
        AssetDatabase.FindAssets("t:AudioClip", new[] { Audio + folder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => System.IO.Path.GetFileName(p).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p)
            .Select(p => AssetDatabase.LoadAssetAtPath<AudioClip>(p))
            .ToArray();

    private static bool Empty(AudioClip[] clips) => clips == null || clips.Length == 0 || clips.All(c => c == null);

    private static T Load<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new InvalidOperationException("Missing " + path);
        return asset;
    }
}
