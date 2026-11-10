using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Sets up the sword attack animation for the two Ranger test scenes.
// Prototype, RangerLocomotion and the preview controller aren't changed.
public static class PrepareCombatAnimations
{
    private const string LibraryPath = "Assets/ThirdParty/Quaternius/AnimationLibrary2/Models/UAL2_Standard.fbx";
    private const string ControllerPath = "Assets/Animations/Player/RangerDirectional.controller";
    private const string MaskPath = "Assets/Animations/Player/UpperBody.mask";
    private const string AttackClipName = "Sword_Regular_A";
    private const string LayerName = "Combat";
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Tests/RangerMovementTest.unity",
        "Assets/Scenes/Tests/RangerMovementTestFemale.unity",
    };

    [MenuItem("Tools/Portaland/Prepare Combat Animations")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        try
        {
            ImportAsHumanoid();
            AnimationClip attack = FindClip(AttackClipName);
            AvatarMask mask = GetOrCreateUpperBodyMask();
            AddCombatLayer(attack, mask);
            AssetDatabase.SaveAssets();
            AddToTestScenes(attack);
            Debug.Log("Combat animations ready. Open a Ranger movement test scene, press Play and left click to attack.");
        }
        catch (Exception error)
        {
            Debug.LogError("Combat animation setup stopped: " + error.Message);
        }
    }

    // Same import style as the first library: Humanoid, in place, keeping the original height.
    private static void ImportAsHumanoid()
    {
        ModelImporter importer = AssetImporter.GetAtPath(LibraryPath) as ModelImporter;
        if (importer == null) throw new InvalidOperationException("Missing " + LibraryPath);

        if (importer.animationType != ModelImporterAnimationType.Human || importer.clipAnimations.Length == 0)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            importer.SaveAndReimport();

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            foreach (ModelImporterClipAnimation clip in clips)
            {
                clip.loopTime = clip.name.Contains("_Loop");
                clip.lockRootRotation = true;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalPositionY = true;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(LibraryPath).OfType<Avatar>().FirstOrDefault();
        if (avatar == null || !avatar.isValid || !avatar.isHuman)
            throw new InvalidOperationException("UAL2 didn't import as a valid Humanoid. Check its Rig tab in the Inspector.");
    }

    private static AnimationClip FindClip(string clipName)
    {
        AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(LibraryPath).OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__") && (c.name == clipName || c.name.EndsWith("|" + clipName)));
        if (clip == null) throw new InvalidOperationException("Couldn't find the " + clipName + " clip in " + LibraryPath);
        return clip;
    }

    // Arms, head and body only, so the legs keep walking while the arms attack.
    private static AvatarMask GetOrCreateUpperBodyMask()
    {
        AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(MaskPath);
        if (mask != null) return mask;

        mask = new AvatarMask();
        for (AvatarMaskBodyPart part = 0; part < AvatarMaskBodyPart.LastBodyPart; part++)
        {
            bool lowerBody = part == AvatarMaskBodyPart.Root || part == AvatarMaskBodyPart.LeftLeg
                || part == AvatarMaskBodyPart.RightLeg || part == AvatarMaskBodyPart.LeftFootIK
                || part == AvatarMaskBodyPart.RightFootIK;
            mask.SetHumanoidBodyPartActive(part, !lowerBody);
        }
        AssetDatabase.CreateAsset(mask, MaskPath);
        return mask;
    }

    // A Combat layer on top of movement: it sits empty until the Attack trigger fires.
    private static void AddCombatLayer(AnimationClip attack, AvatarMask mask)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) throw new InvalidOperationException("Missing " + ControllerPath);

        if (!controller.parameters.Any(p => p.name == "Attack"))
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

        AnimatorControllerLayer[] layers = controller.layers;
        int index = Array.FindIndex(layers, layer => layer.name == LayerName);
        if (index < 0)
        {
            controller.AddLayer(LayerName);
            layers = controller.layers;
            index = layers.Length - 1;

            AnimatorStateMachine machine = layers[index].stateMachine;
            AnimatorState empty = machine.AddState("Empty");
            machine.defaultState = empty;
            AnimatorState attackState = machine.AddState("Attack");
            attackState.motion = attack;

            AnimatorStateTransition start = empty.AddTransition(attackState);
            start.hasExitTime = false;
            start.duration = 0.05f;
            start.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            AnimatorStateTransition finish = attackState.AddTransition(empty);
            finish.hasExitTime = true;
            finish.exitTime = 0.85f;
            finish.duration = 0.15f;
        }

        layers[index].avatarMask = mask;
        layers[index].defaultWeight = 1f;
        layers[index].blendingMode = AnimatorLayerBlendingMode.Override;
        controller.layers = layers;
        EditorUtility.SetDirty(controller);
    }

    private static void AddToTestScenes(AnimationClip attack)
    {
        foreach (string path in ScenePaths)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            bool wasOpen = scene.isLoaded;
            if (!wasOpen) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            PlayerCombat[] players = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PlayerCombat>(true)).ToArray();
            if (players.Length != 1)
                throw new InvalidOperationException("Expected one Player with PlayerCombat in " + path + "; found " + players.Length);
            PlayerCombat combat = players[0];

            PlayerCombatAnimation animation = combat.GetComponent<PlayerCombatAnimation>();
            if (animation == null) animation = combat.gameObject.AddComponent<PlayerCombatAnimation>();
            PlayerLocomotionAnimation locomotion = combat.GetComponent<PlayerLocomotionAnimation>();
            if (animation.characterAnimator == null && locomotion != null)
                animation.characterAnimator = locomotion.characterAnimator;

            // Attacks last about as long as the animation, until it starts blending back out.
            combat.handAttackDuration = attack.length * 0.85f;

            EditorUtility.SetDirty(combat);
            EditorUtility.SetDirty(animation);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
