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
    private const string FirstLibraryPath = "Assets/ThirdParty/Quaternius/AnimationLibrary/Models/UAL1_Standard.fbx";
    private const string ControllerPath = "Assets/Animations/Player/RangerDirectional.controller";
    private const string MaskPath = "Assets/Animations/Player/UpperBody.mask";
    private const string AttackClipName = "Sword_Regular_A";
    private const string LayerName = "Combat";

    // Clip, how far into it the blade connects, damage multiplier and stamina cost.
    // Light attacks chain in order when tapping; the heavy is a slow, strong single hit.
    private static readonly (string clip, float hitPoint, float damage, float stamina)[] LightAttacks =
    {
        ("Sword_Regular_A", 0.42f, 1f, 10f),
        ("Sword_Regular_B", 0.42f, 1f, 10f),
    };
    private static readonly (string clip, float hitPoint, float damage, float stamina) HeavyAttack =
        ("Sword_Regular_C", 0.4f, 2f, 25f);
    // Overhead swing from the first library, used when attacking in the air.
    private static readonly (string clip, float hitPoint, float damage, float stamina) JumpAttack =
        ("Sword_Attack", 0.35f, 1.5f, 15f);
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
            AnimationClip[] lightClips = LightAttacks.Select(hit => FindClip(hit.clip)).ToArray();
            AnimationClip heavyClip = FindClip(HeavyAttack.clip);
            AnimationClip jumpClip = FindClip(JumpAttack.clip, FirstLibraryPath);
            AddAttackStates(lightClips.Append(heavyClip).Append(jumpClip).ToArray());
            AssetDatabase.SaveAssets();
            AddToTestScenes(attack, lightClips, heavyClip, jumpClip);
            Debug.Log("Combat animations ready. Open a Ranger movement test scene, press Play, tap left click for light attacks, hold it for a heavy attack, or click in the air for a jump attack.");
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

    private static AnimationClip FindClip(string clipName, string path = LibraryPath)
    {
        AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__") && (c.name == clipName || c.name.EndsWith("|" + clipName)));
        if (clip == null) throw new InvalidOperationException("Couldn't find the " + clipName + " clip in " + path);
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

    // One Combat layer state per attack clip, named after the clip, which blends back to Empty.
    private static void AddAttackStates(AnimationClip[] clips)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        AnimatorStateMachine machine = controller.layers.First(layer => layer.name == LayerName).stateMachine;
        AnimatorState empty = machine.states.Select(child => child.state).First(state => state.name == "Empty");

        foreach (AnimationClip clip in clips)
        {
            string stateName = ShortName(clip);
            if (machine.states.Any(child => child.state.name == stateName)) continue;

            AnimatorState state = machine.AddState(stateName);
            state.motion = clip;
            AnimatorStateTransition finish = state.AddTransition(empty);
            finish.hasExitTime = true;
            finish.exitTime = 0.85f;
            finish.duration = 0.15f;
        }
        EditorUtility.SetDirty(controller);
    }

    private static string ShortName(AnimationClip clip) => clip.name.Split('|').Last();

    private static HandAttack MakeAttack(AnimationClip clip, (string clip, float hitPoint, float damage, float stamina) settings)
    {
        return new HandAttack
        {
            name = ShortName(clip),
            duration = clip.length * 0.85f,
            hitDelay = clip.length * settings.hitPoint,
            damageMultiplier = settings.damage,
            staminaCost = settings.stamina,
        };
    }

    private static void AddToTestScenes(AnimationClip attack, AnimationClip[] lightClips, AnimationClip heavyClip, AnimationClip jumpClip)
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

            // Filled once; after that, tweaks made in the Inspector are kept.
            if (combat.lightAttacks == null || combat.lightAttacks.Length == 0)
                combat.lightAttacks = lightClips.Select((clip, i) => MakeAttack(clip, LightAttacks[i])).ToArray();
            if (combat.heavyAttack == null || string.IsNullOrEmpty(combat.heavyAttack.name))
                combat.heavyAttack = MakeAttack(heavyClip, HeavyAttack);
            if (combat.jumpAttack == null || string.IsNullOrEmpty(combat.jumpAttack.name))
                combat.jumpAttack = MakeAttack(jumpClip, JumpAttack);

            EditorUtility.SetDirty(combat);
            EditorUtility.SetDirty(animation);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
