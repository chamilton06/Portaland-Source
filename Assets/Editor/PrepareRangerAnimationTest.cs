using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Makes a separate test scene so we can check the models before touching Player.
[InitializeOnLoad]
public static class PrepareRangerAnimationTest
{
    private const string AnimationPath = "Assets/ThirdParty/Quaternius/AnimationLibrary/Models/UAL1_Standard.fbx";
    private const string ControllerPath = "Assets/Animations/Player/RangerPreview.controller";
    private const string ScenePath = "Assets/Scenes/Tests/RangerAnimationTest.unity";

    static PrepareRangerAnimationTest()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool("Portaland.AnimationTest.Attempted.v2", false)) return;
            SessionState.SetBool("Portaland.AnimationTest.Attempted.v2", true);
            Prepare();
        };
    }

    [MenuItem("Tools/Portaland/Prepare Ranger Animation Test")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        try
        {
            ConfigureAnimations();
            MakeFolder("Assets/Animations/Player");
            MakeFolder("Assets/Scenes/Tests");
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null) controller = CreateController();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) CreateScene(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("Ranger animation test ready: " + ScenePath + ". Open it and press Play. Use 1–4 to test both characters.");
        }
        catch (Exception error)
        {
            Debug.LogError("Ranger animation test setup stopped: " + error);
        }
    }

    private static void ConfigureAnimations()
    {
        ModelImporter importer = AssetImporter.GetAtPath(AnimationPath) as ModelImporter;
        if (importer == null) throw new InvalidOperationException("Animation FBX wasn't found.");
        bool changed = importer.animationType != ModelImporterAnimationType.Human
            || importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel;
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips.Length == 0) { clips = importer.defaultClipAnimations; changed = true; }
        if (clips.Length == 0) throw new InvalidOperationException("No clips found in the animation FBX.");
        foreach (ModelImporterClipAnimation clip in clips)
        {
            if (!IsPreviewClip(clip.name)) continue;
            if (!clip.loopTime || !clip.lockRootPositionXZ || !clip.lockRootRotation) changed = true;
            clip.loopTime = true;
            clip.lockRootPositionXZ = true;
            clip.lockRootRotation = true;
        }
        if (changed) { importer.clipAnimations = clips; importer.SaveAndReimport(); }
        Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(AnimationPath).OfType<Avatar>().FirstOrDefault();
        if (avatar == null || !avatar.isHuman || !avatar.isValid)
            throw new InvalidOperationException("The animation library's Humanoid Avatar didn't validate. Check its import messages.");
    }

    private static bool IsPreviewClip(string name)
    {
        string shortName = name.Split('|').Last();
        return new[] { "Idle_Loop", "Walk_Loop", "Jog_Fwd_Loop", "Sprint_Loop" }.Contains(shortName);
    }

    private static AnimatorController CreateController()
    {
        string[] names = { "Idle_Loop", "Walk_Loop", "Jog_Fwd_Loop", "Sprint_Loop" };
        AnimationClip[] all = AssetDatabase.LoadAllAssetsAtPath(AnimationPath).OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
        // Unity also exposes hidden preview clips. Pick the real exported take first.
        AnimationClip[] clips = names.Select(name => FindClip(all, name)).ToArray();
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        var tree = new BlendTree { name = "Preview Locomotion", blendType = BlendTreeType.Simple1D,
            blendParameter = "Speed", useAutomaticThresholds = false };
        AssetDatabase.AddObjectToAsset(tree, controller);
        for (int i = 0; i < clips.Length; i++) tree.AddChild(clips[i], i);
        AnimatorState state = controller.layers[0].stateMachine.AddState("Locomotion");
        state.motion = tree;
        state.writeDefaultValues = false;
        controller.layers[0].stateMachine.defaultState = state;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static AnimationClip FindClip(AnimationClip[] clips, string name)
    {
        AnimationClip[] matches = clips.Where(clip => clip.name == "Armature|" + name).ToArray();
        if (matches.Length == 0) matches = clips.Where(clip => clip.name == name).ToArray();
        if (matches.Length == 0) matches = clips.Where(clip => clip.name.Split('|').Last() == name).ToArray();
        if (matches.Length != 1)
            throw new InvalidOperationException("Expected one playable clip for " + name + ", found "
                + matches.Length + ". Available clips: " + string.Join(", ", clips.Select(clip => clip.name)));
        return matches[0];
    }

    private static void CreateScene(AnimatorController controller)
    {
        Scene original = SceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            SceneManager.SetActiveScene(scene);
            var animators = new Animator[2];
            string[] sexes = { "Male", "Female" };
            for (int i = 0; i < sexes.Length; i++)
            {
                string path = "Assets/Art/Characters/Player/PlayerRanger" + sexes[i] + ".prefab";
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null) throw new InvalidOperationException("Missing character prefab: " + path);
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
                model.transform.position = new Vector3(i == 0 ? -1.2f : 1.2f, 0f, 0f);
                animators[i] = model.GetComponent<Animator>();
                if (animators[i] == null || animators[i].avatar == null || !animators[i].avatar.isValid)
                    throw new InvalidOperationException("Character Avatar isn't ready: " + path);
                animators[i].runtimeAnimatorController = controller;
                animators[i].applyRootMotion = false;
                animators[i].cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
            new GameObject("Preview Controls").AddComponent<RangerAnimationPreview>().characters = animators;
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Preview Floor";
            floor.transform.localScale = Vector3.one * 2f;
            var light = new GameObject("Preview Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 1.6f, 5f);
            camera.transform.LookAt(new Vector3(0f, 1f, 0f));
            camera.fieldOfView = 45f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.22f, 0.24f, 0.27f);
            camera.gameObject.AddComponent<AudioListener>();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Couldn't save the test scene.");
        }
        finally
        {
            if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void MakeFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        MakeFolder(parent);
        AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
    }
}
