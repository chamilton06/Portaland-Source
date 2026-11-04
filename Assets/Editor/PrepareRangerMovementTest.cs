using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Keep the first gameplay test in its own scene so the group's Prototype stays intact.
public static class PrepareRangerMovementTest
{
    private const string ScenePath = "Assets/Scenes/Tests/RangerMovementTest.unity";
    private const string ControllerPath = "Assets/Animations/Player/RangerLocomotion.controller";


    [MenuItem("Tools/Portaland/Prepare Ranger Movement Test")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
        {
            try
            {
                AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
                AddAirborneStates(existing);
                AddCrouchState(existing);
                EnableAnimatedCrouch();
                AssetDatabase.SaveAssets();
                Debug.Log("Ranger crouch setup ready. Open " + ScenePath + " and test Ctrl and low ceilings. If the scene was already open, save its changes before closing it.");
            }
            catch (Exception error) { Debug.LogError("Ranger movement/crouch setup stopped: " + error); }
            return;
        }
        Scene original = SceneManager.GetActiveScene();
        Scene test = default;
        bool copiedScene = false;
        bool copiedController = false;
        bool finished = false;
        try
        {
            string prefabPath = "Assets/Art/Characters/Player/PlayerRangerMale.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) throw new InvalidOperationException("Prepare the Ranger prefabs first.");
            if (!AssetDatabase.IsValidFolder("Assets/Scenes/Tests"))
                throw new InvalidOperationException("Prepare the animation test first to create the test folders.");
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) == null)
            {
                copiedController = AssetDatabase.CopyAsset("Assets/Animations/Player/RangerPreview.controller", ControllerPath);
                if (!copiedController) throw new InvalidOperationException("The preview controller wasn't found. Prepare the animation test first.");
            }
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            AddAirborneStates(controller);
            AddCrouchState(controller);
            copiedScene = AssetDatabase.CopyAsset("Assets/Scenes/Prototype.unity", ScenePath);
            if (!copiedScene) throw new InvalidOperationException("Couldn't copy the saved Prototype scene.");
            test = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(test);
            PlayerMovement[] players = test.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PlayerMovement>(true)).ToArray();
            if (players.Length != 1) throw new InvalidOperationException("Expected one gameplay Player in Prototype; found " + players.Length);
            PlayerMovement movement = players[0];
            movement.resizeControllerForCrouch = true;
            GameObject player = movement.gameObject;
            CharacterController capsule = player.GetComponent<CharacterController>();
            if (capsule == null) throw new InvalidOperationException("Player has no CharacterController.");

            // Only remove the three named preview objects from this copied scene.
            foreach (GameObject root in test.GetRootGameObjects())
                if (root != player && (root.name == "Male_Ranger" || root.name == "PlayerRangerMale" || root.name == "PlayerRangerFemale"))
                    Object.DestroyImmediate(root);

            GameObject pivot = new GameObject("CharacterVisual");
            pivot.transform.SetParent(player.transform, false);
            pivot.transform.localPosition = capsule.center;
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, test);
            model.transform.SetParent(pivot.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>().Where(renderer => renderer.enabled).ToArray();
            if (renderers.Length == 0) throw new InvalidOperationException("Character has no visible renderers.");
            // Convert renderer bounds to Player space so parent scale doesn't affect fitting.
            Bounds bounds = LocalBounds(player.transform, renderers);
            if (bounds.size.y <= 0.01f) throw new InvalidOperationException("Character bounds aren't usable.");
            model.transform.localScale *= capsule.height / bounds.size.y;
            bounds = LocalBounds(player.transform, renderers);
            model.transform.localPosition += Vector3.up * (capsule.center.y - capsule.height * 0.5f - bounds.min.y);

            Animator animator = model.GetComponent<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                throw new InvalidOperationException("Ranger needs a valid Humanoid Avatar.");
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            MeshRenderer placeholder = player.GetComponent<MeshRenderer>();
            if (placeholder != null) placeholder.enabled = false;
            movement.createCapsuleRollVisual = false;
            movement.rollVisual = pivot.transform;
            PlayerLocomotionAnimation driver = player.AddComponent<PlayerLocomotionAnimation>();
            driver.characterAnimator = animator;

            BlendTree tree = controller.layers[0].stateMachine.defaultState.motion as BlendTree;
            if (tree == null || tree.children.Length != 4) throw new InvalidOperationException("Expected the four-clip preview blend tree.");
            ChildMotion[] children = tree.children;
            tree.useAutomaticThresholds = false;
            children[0].threshold = 0f;
            children[1].threshold = movement.walkSpeed;
            children[2].threshold = Mathf.Lerp(movement.walkSpeed, movement.sprintSpeed, 0.5f);
            children[3].threshold = movement.sprintSpeed;
            tree.children = children;
            tree.name = "Movement Speed";
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(test)) throw new InvalidOperationException("Couldn't save the movement scene.");
            finished = true;
            Debug.Log("Ranger movement test ready: " + ScenePath + ". Open it and test WASD, Shift, Space and Ctrl.");
        }
        catch (Exception error) { Debug.LogError("Ranger movement setup stopped: " + error); }
        finally
        {
            if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            if (test.IsValid() && test.isLoaded) EditorSceneManager.CloseScene(test, true);
            if (!finished)
            {
                if (copiedScene) AssetDatabase.DeleteAsset(ScenePath);
                if (copiedController) AssetDatabase.DeleteAsset(ControllerPath);
            }
        }
    }

    private static void AddAirborneStates(AnimatorController controller)
    {
        if (controller == null) throw new InvalidOperationException("The gameplay Animator Controller wasn't found.");
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        string[] stateNames = { "Jump Start", "Falling", "Landing" };
        int present = machine.states.Count(state => stateNames.Contains(state.state.name));
        if (present == stateNames.Length) return;
        if (present != 0) throw new InvalidOperationException("Some airborne states already exist. Check the controller before rebuilding them.");
        string path = "Assets/ThirdParty/Quaternius/AnimationLibrary/Models/UAL1_Standard.fbx";
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null) throw new InvalidOperationException("The animation library wasn't found.");
        ModelImporterClipAnimation[] settings = importer.clipAnimations;
        if (settings.Length == 0) settings = importer.defaultClipAnimations;
        string[] names = { "Jump_Start", "Jump_Loop", "Jump_Land" };
        foreach (string name in names)
            if (!settings.Any(clip => clip.name.Split('|').Last() == name))
                throw new InvalidOperationException("Missing imported clip settings: " + name);
        foreach (ModelImporterClipAnimation clip in settings)
        {
            string name = clip.name.Split('|').Last();
            if (!names.Contains(name)) continue;
            clip.loopTime = name == "Jump_Loop";
            clip.lockRootPositionXZ = true;
            clip.lockRootRotation = true;
        }
        importer.clipAnimations = settings;
        importer.SaveAndReimport();
        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
        AnimationClip Find(string name)
        {
            AnimationClip[] matches = clips.Where(clip => clip.name == "Armature|" + name).ToArray();
            if (matches.Length == 0) matches = clips.Where(clip => clip.name == name).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("Expected one playable " + name + " clip; found " + matches.Length);
            return matches[0];
        }
        // Find every clip before changing the controller so a missing clip doesn't leave half a setup.
        AnimationClip startClip = Find("Jump_Start");
        AnimationClip airClip = Find("Jump_Loop");
        AnimationClip landClip = Find("Jump_Land");
        AnimatorState locomotion = machine.defaultState;
        if (locomotion == null) throw new InvalidOperationException("No default locomotion state.");
        if (!controller.parameters.Any(parameter => parameter.name == "Grounded"))
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        if (!controller.parameters.Any(parameter => parameter.name == "VerticalSpeed"))
            controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
        AnimatorState jump = machine.AddState("Jump Start", new Vector3(330, -100));
        AnimatorState fall = machine.AddState("Falling", new Vector3(570, -100));
        AnimatorState land = machine.AddState("Landing", new Vector3(570, 100));
        jump.motion = startClip;
        fall.motion = airClip;
        land.motion = landClip;
        jump.writeDefaultValues = fall.writeDefaultValues = land.writeDefaultValues = false;

        // Movement decides when we leave the floor. Animation never applies the jump force.
        foreach (AnimatorState from in new[] { locomotion, land })
        {
            AnimatorStateTransition takeoff = Transition(from, jump);
            takeoff.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            takeoff.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VerticalSpeed");
            AnimatorStateTransition drop = Transition(from, fall);
            drop.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            drop.AddCondition(AnimatorConditionMode.Less, 0.1f, "VerticalSpeed");
        }
        AnimatorStateTransition apex = Transition(jump, fall);
        apex.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
        apex.AddCondition(AnimatorConditionMode.Less, 0.1f, "VerticalSpeed");
        foreach (AnimatorState from in new[] { jump, fall })
            Transition(from, land).AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
        AnimatorStateTransition finishLanding = Transition(land, locomotion);
        finishLanding.hasExitTime = true;
        finishLanding.exitTime = 0.85f;
        finishLanding.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
        // Walking can interrupt the landing pose, so the controller never feels locked in place.
        AnimatorStateTransition movingLanding = Transition(land, locomotion);
        movingLanding.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
        movingLanding.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        EditorUtility.SetDirty(controller);
    }

    private static AnimatorStateTransition Transition(AnimatorState from, AnimatorState to)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = 0.08f;
        return transition;
    }

    private static void AddCrouchState(AnimatorController controller)
    {
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        if (machine.states.Any(child => child.state.name == "Crouching")) return;
        const string path = "Assets/ThirdParty/Quaternius/AnimationLibrary/Models/UAL1_Standard.fbx";
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null) throw new InvalidOperationException("Missing animation importer.");
        string[] names = { "Crouch_Idle_Loop", "Crouch_Fwd_Loop" };
        ModelImporterClipAnimation[] settings = importer.clipAnimations;
        if (settings.Length == 0) settings = importer.defaultClipAnimations;
        foreach (string name in names)
            if (!settings.Any(clip => clip.name.Split('|').Last() == name))
                throw new InvalidOperationException("Missing crouch clip: " + name);
        foreach (ModelImporterClipAnimation clip in settings)
        {
            if (!names.Contains(clip.name.Split('|').Last())) continue;
            clip.loopTime = true;
            clip.lockRootPositionXZ = true;
            clip.lockRootRotation = true;
        }
        importer.clipAnimations = settings;
        importer.SaveAndReimport();
        AnimationClip[] all = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
        AnimationClip Find(string name)
        {
            AnimationClip[] matches = all.Where(clip => clip.name == "Armature|" + name).ToArray();
            if (matches.Length == 0) matches = all.Where(clip => clip.name == name).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("Expected one " + name + "; found " + matches.Length);
            return matches[0];
        }
        AnimationClip idle = Find(names[0]);
        AnimationClip walk = Find(names[1]);
        AnimatorState locomotion = machine.defaultState;
        AnimatorState landing = machine.states.Single(child => child.state.name == "Landing").state;
        AnimatorState falling = machine.states.Single(child => child.state.name == "Falling").state;
        if (!controller.parameters.Any(parameter => parameter.name == "Crouching"))
            controller.AddParameter("Crouching", AnimatorControllerParameterType.Bool);
        if (!controller.parameters.Any(parameter => parameter.name == "CrouchSpeed"))
            controller.AddParameter("CrouchSpeed", AnimatorControllerParameterType.Float);
        var tree = new BlendTree { name = "Crouch Movement", blendType = BlendTreeType.Simple1D,
            blendParameter = "CrouchSpeed", useAutomaticThresholds = false };
        AssetDatabase.AddObjectToAsset(tree, controller);
        tree.AddChild(idle, 0f);
        tree.AddChild(walk, 1f);
        AnimatorState crouch = machine.AddState("Crouching", new Vector3(330, 250));
        crouch.motion = tree;
        crouch.writeDefaultValues = false;
        foreach (AnimatorState from in new[] { locomotion, landing })
        {
            AnimatorStateTransition enter = Transition(from, crouch);
            enter.AddCondition(AnimatorConditionMode.If, 0f, "Crouching");
            enter.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
        }
        foreach (AnimatorStateTransition exitLanding in landing.transitions.Where(transition => transition.destinationState == locomotion))
            if (!exitLanding.conditions.Any(condition => condition.parameter == "Crouching"))
                exitLanding.AddCondition(AnimatorConditionMode.IfNot, 0f, "Crouching");
        AnimatorStateTransition stand = Transition(crouch, locomotion);
        stand.AddCondition(AnimatorConditionMode.IfNot, 0f, "Crouching");
        stand.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
        Transition(crouch, falling).AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
        EditorUtility.SetDirty(controller);
    }

    private static void EnableAnimatedCrouch()
    {
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool wasOpen = scene.IsValid() && scene.isLoaded;
        if (!wasOpen) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        try
        {
            PlayerMovement[] players = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PlayerMovement>(true)).ToArray();
            if (players.Length != 1) throw new InvalidOperationException("Expected one Player for crouch setup.");
            if (players[0].resizeControllerForCrouch) return;
            players[0].resizeControllerForCrouch = true;
            EditorUtility.SetDirty(players[0]);
            EditorSceneManager.MarkSceneDirty(scene);
            // If it's open, leave saving to the user so we don't save other unfinished edits.
            if (!wasOpen && !EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Couldn't save animated crouch settings.");
        }
        finally { if (!wasOpen) EditorSceneManager.CloseScene(scene, true); }
    }

    private static Bounds LocalBounds(Transform player, Renderer[] renderers)
    {
        Bounds result = default;
        bool first = true;
        foreach (Renderer renderer in renderers)
        {
            Bounds world = renderer.bounds;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 point = player.InverseTransformPoint(world.center + Vector3.Scale(world.extents, new Vector3(x, y, z)));
                        if (first) { result = new Bounds(point, Vector3.zero); first = false; }
                        else result.Encapsulate(point);
                    }
        }
        return result;
    }
}
