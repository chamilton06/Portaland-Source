using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Reuse the same animation setup for male and female gameplay tests.
public static class PrepareRangerRollTest
{
    private const string MaleScene = "Assets/Scenes/Tests/RangerMovementTest.unity";
    private const string FemaleScene = "Assets/Scenes/Tests/RangerMovementTestFemale.unity";
    private const string DirectionalController = "Assets/Animations/Player/RangerDirectional.controller";

    [MenuItem("Tools/Portaland/Prepare Ranger Roll Tests")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        foreach (string path in new[] { MaleScene, FemaleScene })
        {
            Scene open = SceneManager.GetSceneByPath(path);
            if (open.IsValid() && open.isLoaded && open.isDirty)
                throw new InvalidOperationException("Save your edits in " + path + " first.");
        }
        AnimationClip roll = PrepareClip();
        foreach (string path in new[] { "Assets/Animations/Player/RangerLocomotion.controller", DirectionalController })
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) throw new InvalidOperationException("Missing controller: " + path);
            AddRollState(controller, roll);
        }
        ConfigureScene(MaleScene, false);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(FemaleScene) == null && !AssetDatabase.CopyAsset(MaleScene, FemaleScene))
            throw new InvalidOperationException("Couldn't create the female gameplay test.");
        ConfigureScene(FemaleScene, true);
        AssetDatabase.SaveAssets();
        Debug.Log("Ranger roll tests ready. Male and female gameplay scenes share directional, airborne, crouch and roll states. Test Q and Space in Play mode.");
    }

    private static AnimationClip PrepareClip()
    {
        const string path = "Assets/ThirdParty/Quaternius/AnimationLibrary/Models/UAL1_Standard.fbx";
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null) throw new InvalidOperationException("Animation library wasn't found.");
        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips.Length == 0) clips = importer.defaultClipAnimations;
        ModelImporterClipAnimation[] matches = clips.Where(clip => clip.name.Split('|').Last() == "Roll").ToArray();
        if (matches.Length != 1) throw new InvalidOperationException("Expected one Roll take.");
        matches[0].loopTime = false;
        matches[0].lockRootPositionXZ = true;
        matches[0].lockRootRotation = true;
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
        AnimationClip[] animations = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal)
                && clip.name.Split('|').Last() == "Roll").ToArray();
        if (animations.Length != 1 || !animations[0].humanMotion)
            throw new InvalidOperationException("Roll must import as one valid Humanoid clip.");
        return animations[0];
    }

    private static void AddRollState(AnimatorController controller, AnimationClip clip)
    {
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        if (machine.states.Any(child => child.state.name == "Rolling")) return;
        AnimatorState Find(string name) => machine.states.Single(child => child.state.name == name).state;
        AnimatorState falling = Find("Falling"), crouching = Find("Crouching");
        foreach (var parameter in new[] { new AnimatorControllerParameter { name = "Rolling", type = AnimatorControllerParameterType.Bool },
            new AnimatorControllerParameter { name = "RollRate", type = AnimatorControllerParameterType.Float, defaultFloat = 1f } })
            if (!controller.parameters.Any(existing => existing.name == parameter.name)) controller.AddParameter(parameter);
        AnimatorState roll = machine.AddState("Rolling", new Vector3(70, -200));
        roll.motion = clip;
        roll.writeDefaultValues = false;
        roll.speed = clip.length;
        roll.speedParameter = "RollRate";
        roll.speedParameterActive = true;
        AnimatorStateTransition enter = machine.AddAnyStateTransition(roll);
        enter.hasExitTime = false;
        enter.hasFixedDuration = true;
        enter.duration = 0.035f;
        enter.canTransitionToSelf = false;
        enter.AddCondition(AnimatorConditionMode.If, 0f, "Rolling");
        AnimatorStateTransition drop = Exit(roll, falling);
        drop.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
        AnimatorStateTransition duck = Exit(roll, crouching);
        duck.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
        duck.AddCondition(AnimatorConditionMode.If, 0f, "Crouching");
        AnimatorStateTransition stand = Exit(roll, machine.defaultState);
        stand.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
        stand.AddCondition(AnimatorConditionMode.IfNot, 0f, "Crouching");
        EditorUtility.SetDirty(controller);
    }

    private static AnimatorStateTransition Exit(AnimatorState from, AnimatorState to)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = 0.06f;
        transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "Rolling");
        return transition;
    }

    private static void ConfigureScene(string path, bool female)
    {
        Scene original = SceneManager.GetActiveScene();
        Scene scene = SceneManager.GetSceneByPath(path);
        bool wasOpen = scene.IsValid() && scene.isLoaded;
        if (!wasOpen) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        try
        {
            PlayerLocomotionAnimation[] players = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PlayerLocomotionAnimation>(true)).ToArray();
            if (players.Length != 1) throw new InvalidOperationException("Expected one Player in " + path);
            PlayerLocomotionAnimation driver = players[0];
            PlayerMovement movement = driver.GetComponent<PlayerMovement>();
            CharacterController capsule = driver.GetComponent<CharacterController>();
            Transform pivot = driver.transform.Find("CharacterVisual");
            if (pivot == null || driver.characterAnimator == null) throw new InvalidOperationException("Player visual isn't ready.");
            if (female && driver.characterAnimator.gameObject.name != "PlayerRangerFemale")
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/Player/PlayerRangerFemale.prefab");
                if (prefab == null) throw new InvalidOperationException("Female Ranger prefab wasn't found.");
                GameObject oldVisual = driver.characterAnimator.gameObject;
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                model.transform.SetParent(pivot, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                Renderer[] renderers = model.GetComponentsInChildren<Renderer>().Where(renderer => renderer.enabled).ToArray();
                Bounds bounds = LocalBounds(driver.transform, renderers);
                if (bounds.size.y < 0.01f) throw new InvalidOperationException("Female model bounds aren't usable.");
                model.transform.localScale *= capsule.height / bounds.size.y;
                bounds = LocalBounds(driver.transform, renderers);
                model.transform.localPosition += Vector3.up * (capsule.center.y - capsule.height * 0.5f - bounds.min.y);
                driver.characterAnimator = model.GetComponent<Animator>();
                Object.DestroyImmediate(oldVisual);
            }
            driver.characterAnimator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(DirectionalController);
            driver.characterAnimator.applyRootMotion = false;
            driver.rollFacing = pivot;
            movement.rollVisual = null;
            movement.createCapsuleRollVisual = false;
            movement.resizeControllerForCrouch = true;
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Couldn't save " + path);
        }
        finally
        {
            if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static Bounds LocalBounds(Transform parent, Renderer[] renderers)
    {
        Bounds result = default;
        bool first = true;
        foreach (Renderer renderer in renderers)
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Bounds bounds = renderer.bounds;
                        Vector3 point = parent.InverseTransformPoint(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z)));
                        if (first) { result = new Bounds(point, Vector3.zero); first = false; }
                        else result.Encapsulate(point);
                    }
        return result;
    }
}
