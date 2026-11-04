using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Only builds when real directional clips have been supplied. No changes on script reload.
public class RangerDirectionalSetup : EditorWindow
{
    private const string Source = "Assets/Animations/Player/RangerLocomotion.controller";
    private const string Output = "Assets/Animations/Player/RangerDirectional.controller";
    private const string ClipSettings = "Assets/Animations/Player/RangerDirectionalClips.asset";
    private const string KayKitPath = "Assets/ThirdParty/KayKit/CharacterAnimations/Rig_Medium_MovementAdvanced.fbx";
    private PlayerDirectionalClips clips;
    private Editor settingsEditor;
    private string message;

    [MenuItem("Tools/Portaland/Set Up Directional Animations")]
    public static void Open() => GetWindow<RangerDirectionalSetup>("Ranger Directions");

    [MenuItem("Tools/Portaland/Prepare Directional Movement Test")]
    public static void PrepareMovementTest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before preparing the test.");
        const string scenePath = "Assets/Scenes/Tests/RangerMovementTest.unity";
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        bool wasOpen = scene.IsValid() && scene.isLoaded;
        if (wasOpen && scene.isDirty)
            throw new InvalidOperationException("Save the movement test scene before running this command.");
        Scene original = SceneManager.GetActiveScene();
        var setup = CreateInstance<RangerDirectionalSetup>();
        try
        {
            setup.PrepareKayKit();
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(Output) == null) setup.Build();
            if (!wasOpen) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            PlayerLocomotionAnimation[] players = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PlayerLocomotionAnimation>(true)).ToArray();
            if (players.Length != 1 || players[0].characterAnimator == null)
                throw new InvalidOperationException("Expected one Player animation driver with a character Animator in the test scene.");
            Animator animator = players[0].characterAnimator;
            Undo.RecordObject(animator, "Set up directional movement test");
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(Output);
            animator.applyRootMotion = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Couldn't save the directional test scene.");
            AssetDatabase.SaveAssets();
            Debug.Log("Directional movement test ready: " + scenePath + ". KayKit Avatar, three loop clips and controller assignment validated. Play mode appearance still needs checking.");
        }
        finally
        {
            DestroyImmediate(setup);
            if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            if (!wasOpen && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
        }
    }

    void OnEnable() { clips = AssetDatabase.LoadAssetAtPath<PlayerDirectionalClips>(ClipSettings); }
    void OnDisable() { if (settingsEditor != null) DestroyImmediate(settingsEditor); }

    void OnGUI()
    {
        EditorGUILayout.HelpBox("Assign real Humanoid strafe/backwards clips. The current free Standard pack doesn't contain them. The working movement controller stays available.", MessageType.Info);
        if (GUILayout.Button("Prepare free KayKit clips and build controller"))
        {
            try
            {
                PrepareKayKit();
                if (AssetDatabase.LoadAssetAtPath<AnimatorController>(Output) == null) Build();
                message = "KayKit directions are ready. Select the gameplay Player and assign the controller below. Test pacing and clothing in Play mode.";
            }
            catch (Exception error) { message = error.Message; Debug.LogWarning(message); }
        }
        clips = (PlayerDirectionalClips)EditorGUILayout.ObjectField("Clip choices", clips, typeof(PlayerDirectionalClips), false);
        if (clips == null)
        {
            if (GUILayout.Button("Create clip choices asset"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Animations/Player"))
                { message = "Prepare the Ranger movement test first."; return; }
                clips = AssetDatabase.LoadAssetAtPath<PlayerDirectionalClips>(ClipSettings);
                if (clips == null)
                {
                    clips = CreateInstance<PlayerDirectionalClips>();
                    AssetDatabase.CreateAsset(clips, ClipSettings);
                    AssetDatabase.SaveAssets();
                }
            }
        }
        else
        {
            Editor.CreateCachedEditor(clips, null, ref settingsEditor);
            settingsEditor.OnInspectorGUI();
            if (GUILayout.Button("Build directional controller"))
            {
                try { Build(); message = "Controller created. Select the gameplay Player to assign it below."; }
                catch (Exception error) { message = error.Message; Debug.LogWarning(message); }
            }
        }
        using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            if (GUILayout.Button("Use controller on selected Player")) Assign();
        if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
    }

    private void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        Validate(clips.walkLeft, "Walk Left");
        Validate(clips.walkRight, "Walk Right");
        Validate(clips.walkBack, "Walk Back");
        bool anyCrouch = clips.crouchLeft != null || clips.crouchRight != null || clips.crouchBack != null;
        if (anyCrouch)
        {
            Validate(clips.crouchLeft, "Crouch Left");
            Validate(clips.crouchRight, "Crouch Right");
            Validate(clips.crouchBack, "Crouch Back");
        }
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(Output) != null)
            throw new InvalidOperationException("The directional controller already exists. Edit its blend trees in the Animator window; this tool won't overwrite it.");
        if (!AssetDatabase.CopyAsset(Source, Output)) throw new InvalidOperationException("Prepare the movement controller first.");
        try
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Output);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AddTree(controller, machine.defaultState, clips.walkLeft, clips.walkRight, clips.walkBack, "MoveX", "MoveZ");
            if (anyCrouch)
            {
                AnimatorState crouch = machine.states.Single(child => child.state.name == "Crouching").state;
                AddTree(controller, crouch, clips.crouchLeft, clips.crouchRight, clips.crouchBack, "CrouchX", "CrouchZ");
            }
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
        catch { AssetDatabase.DeleteAsset(Output); throw; }
    }

    private void PrepareKayKit()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        var importer = AssetImporter.GetAtPath(KayKitPath) as ModelImporter;
        if (importer == null) throw new InvalidOperationException("Wait for the KayKit FBX to import first.");
        // KayKit calls the joint at the end of the forearm 'wrist', so map that as Hand.
        string[,] mapping = {
            { "Hips", "hips" }, { "Spine", "spine" }, { "Chest", "chest" }, { "Head", "head" },
            { "LeftUpperArm", "upperarm.l" }, { "LeftLowerArm", "lowerarm.l" }, { "LeftHand", "wrist.l" },
            { "RightUpperArm", "upperarm.r" }, { "RightLowerArm", "lowerarm.r" }, { "RightHand", "wrist.r" },
            { "LeftUpperLeg", "upperleg.l" }, { "LeftLowerLeg", "lowerleg.l" }, { "LeftFoot", "foot.l" }, { "LeftToes", "toes.l" },
            { "RightUpperLeg", "upperleg.r" }, { "RightLowerLeg", "lowerleg.r" }, { "RightFoot", "foot.r" }, { "RightToes", "toes.r" }
        };
        HumanDescription description = importer.humanDescription;
        description.human = Enumerable.Range(0, mapping.GetLength(0)).Select(i => new HumanBone {
            humanName = mapping[i, 0], boneName = mapping[i, 1], limit = new HumanLimit { useDefaultValues = true }
        }).ToArray();
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.humanDescription = description;
        ModelImporterClipAnimation[] settings = importer.clipAnimations;
        if (settings.Length == 0) settings = importer.defaultClipAnimations;
        string[] names = { "Running_Strafe_Left", "Running_Strafe_Right", "Walking_Backwards" };
        foreach (string name in names)
            if (!settings.Any(clip => clip.name.Split('|').Last() == name))
                throw new InvalidOperationException("KayKit download is missing " + name);
        foreach (ModelImporterClipAnimation clip in settings)
        {
            if (!names.Contains(clip.name.Split('|').Last())) continue;
            clip.loopTime = true;
            clip.lockRootPositionXZ = true;
            clip.lockRootRotation = true;
        }
        importer.clipAnimations = settings;
        importer.SaveAndReimport();
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(KayKitPath);
        Avatar avatar = assets.OfType<Avatar>().FirstOrDefault();
        if (avatar == null || !avatar.isValid || !avatar.isHuman)
            throw new InvalidOperationException("KayKit's Humanoid Avatar didn't validate. Check the FBX Rig tab and import messages.");
        AnimationClip Find(string name)
        {
            AnimationClip[] matches = assets.OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal)
                    && clip.name.Split('|').Last() == name).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("Expected one KayKit clip named " + name + "; found " + matches.Length);
            return matches[0];
        }
        AnimationClip left = Find(names[0]), right = Find(names[1]), back = Find(names[2]);
        if (clips == null) clips = AssetDatabase.LoadAssetAtPath<PlayerDirectionalClips>(ClipSettings);
        if (clips == null)
        {
            clips = CreateInstance<PlayerDirectionalClips>();
            AssetDatabase.CreateAsset(clips, ClipSettings);
        }
        // Keep any clip choices the group has already made.
        if (clips.walkLeft == null) clips.walkLeft = left;
        if (clips.walkRight == null) clips.walkRight = right;
        if (clips.walkBack == null) clips.walkBack = back;
        EditorUtility.SetDirty(clips);
        AssetDatabase.SaveAssets();
    }

    private static void Validate(AnimationClip clip, string label)
    {
        if (clip == null) throw new InvalidOperationException("Assign " + label + " first. No controller was changed.");
        if (!clip.humanMotion) throw new InvalidOperationException(label + " must be imported as a valid Humanoid clip.");
        if (!clip.isLooping) throw new InvalidOperationException("Enable Loop Time and Apply on " + label + " first.");
    }

    private static void AddTree(AnimatorController controller, AnimatorState state,
        AnimationClip left, AnimationClip right, AnimationClip back, string x, string z)
    {
        BlendTree forward = state.motion as BlendTree;
        if (forward == null) throw new InvalidOperationException("Expected the existing speed blend tree on " + state.name);
        ChildMotion idle = forward.children.FirstOrDefault(child => Mathf.Approximately(child.threshold, 0f));
        if (idle.motion == null) throw new InvalidOperationException("No idle motion in " + state.name);
        foreach (string parameter in new[] { x, z })
            if (!controller.parameters.Any(p => p.name == parameter))
                controller.AddParameter(parameter, AnimatorControllerParameterType.Float);
        var tree = new BlendTree { name = state.name + " Directions", blendType = BlendTreeType.FreeformCartesian2D,
            blendParameter = x, blendParameterY = z, useAutomaticThresholds = false };
        AssetDatabase.AddObjectToAsset(tree, controller);
        tree.AddChild(idle.motion, Vector2.zero);
        tree.AddChild(forward, Vector2.up);
        tree.AddChild(back, Vector2.down);
        tree.AddChild(left, Vector2.left);
        tree.AddChild(right, Vector2.right);
        state.motion = tree;
        EditorUtility.SetDirty(tree);
    }

    private void Assign()
    {
        GameObject selected = Selection.activeGameObject;
        PlayerLocomotionAnimation driver = selected != null ? selected.GetComponentInParent<PlayerLocomotionAnimation>() : null;
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Output);
        if (driver == null || driver.characterAnimator == null || controller == null)
        { message = "Select a scene Player with Player Locomotion Animation and a character Animator, then build the controller first."; return; }
        if (EditorUtility.IsPersistent(driver)) { message = "Select the Player in a scene, not a project asset."; return; }
        Animator animator = driver.characterAnimator;
        Undo.RecordObject(animator, "Assign directional animations");
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
        EditorSceneManager.MarkSceneDirty(driver.gameObject.scene);
        message = "Assigned. Save this scene, then test WASD and mouse turning in Play mode.";
    }
}
