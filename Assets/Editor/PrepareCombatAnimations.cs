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
    // Lunging strike while sprinting, and a quick swing while crouched (the legs stay crouched).
    private static readonly (string clip, float hitPoint, float damage, float stamina) SprintAttack =
        ("Sword_Dash", 0.35f, 1.25f, 15f);
    private static readonly (string clip, float hitPoint, float damage, float stamina) CrouchAttack =
        ("Sword_Regular_B", 0.42f, 1f, 10f);

    // Damage each attack can take without flinching (light, sprint and crouch attacks have none).
    private const float HeavyPoise = 25f;
    private const float JumpPoise = 15f;

    // Held guard, quick parry and the stagger when stamina runs out while blocking.
    private const string BlockClipName = "Sword_Block";
    private const string GuardBreakClipName = "Idle_Shield_Break";
    private const float ParrySpeed = 1.5f;
    private const float TestBlockStaminaCost = 15f;

    // Flinch when a hit lands (upper body), and a full-body death on its own layer.
    private const string HitClipName = "Hit_Chest";
    private const string DeathClipName = "Death01";
    private const string DeathLayerName = "Death";
    private const float TestHitStunDuration = 0.4f;
    private const float TestStaggerDuration = 1.5f;
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
            AnimationClip sprintClip = FindClip(SprintAttack.clip);
            AnimationClip crouchClip = FindClip(CrouchAttack.clip);
            AnimationClip guardBreakClip = FindClip(GuardBreakClipName);
            AnimationClip hitClip = FindClip(HitClipName, FirstLibraryPath);
            AddAttackStates(lightClips.Concat(new[] { heavyClip, jumpClip, sprintClip, crouchClip, guardBreakClip, hitClip }).Distinct().ToArray());
            AddGuardStates(FindClip(BlockClipName));
            AddDeathLayer(FindClip(DeathClipName, FirstLibraryPath));
            AssetDatabase.SaveAssets();
            GameObject enemy = GetOrCreateDarkRanger(lightClips[0], lightClips[1], jumpClip, heavyClip);
            AddToTestScenes(attack, lightClips, heavyClip, jumpClip, sprintClip, crouchClip, enemy);
            Debug.Log("Combat animations ready. Open a Ranger movement test scene, press Play, tap left click for light attacks or hold it for heavy. Click while jumping, sprinting or crouched for those attacks.");
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

    // Guard holds one frame of the block clip, picked by GuardTime, so the sword stays raised.
    // Parry plays the whole block clip quickly, then blends back to Empty.
    private static void AddGuardStates(AnimationClip block)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (!controller.parameters.Any(p => p.name == "GuardTime"))
            controller.AddParameter("GuardTime", AnimatorControllerParameterType.Float);

        AnimatorStateMachine machine = controller.layers.First(layer => layer.name == LayerName).stateMachine;
        AnimatorState empty = machine.states.Select(child => child.state).First(state => state.name == "Empty");

        if (!machine.states.Any(child => child.state.name == "Guard"))
        {
            AnimatorState guard = machine.AddState("Guard");
            guard.motion = block;
            guard.timeParameterActive = true;
            guard.timeParameter = "GuardTime";
        }

        if (!machine.states.Any(child => child.state.name == "Parry"))
        {
            AnimatorState parry = machine.AddState("Parry");
            parry.motion = block;
            parry.speed = ParrySpeed;
            AnimatorStateTransition finish = parry.AddTransition(empty);
            finish.hasExitTime = true;
            finish.exitTime = 0.85f;
            finish.duration = 0.15f;
        }
        EditorUtility.SetDirty(controller);
    }

    // A full-body layer above everything else. It stays empty until death, then holds the last frame.
    private static void AddDeathLayer(AnimationClip death)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        AnimatorControllerLayer[] layers = controller.layers;
        int index = Array.FindIndex(layers, layer => layer.name == DeathLayerName);
        if (index >= 0) return;

        controller.AddLayer(DeathLayerName);
        layers = controller.layers;
        index = layers.Length - 1;

        AnimatorStateMachine machine = layers[index].stateMachine;
        machine.defaultState = machine.AddState("Empty");
        machine.AddState(ShortName(death)).motion = death;

        layers[index].defaultWeight = 1f;
        layers[index].blendingMode = AnimatorLayerBlendingMode.Override;
        controller.layers = layers;
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

    private static void AddToTestScenes(AnimationClip attack, AnimationClip[] lightClips, AnimationClip heavyClip,
        AnimationClip jumpClip, AnimationClip sprintClip, AnimationClip crouchClip, GameObject enemyPrefab)
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
            if (combat.sprintAttack == null || string.IsNullOrEmpty(combat.sprintAttack.name))
                combat.sprintAttack = MakeAttack(sprintClip, SprintAttack);
            if (combat.crouchAttack == null || string.IsNullOrEmpty(combat.crouchAttack.name))
                combat.crouchAttack = MakeAttack(crouchClip, CrouchAttack);

            // Poise: the heavy carries on through two dummy hits, the jump attack through one.
            // Only set while 0, so values tuned in the Inspector are kept.
            if (combat.heavyAttack.poise <= 0f) combat.heavyAttack.poise = HeavyPoise;
            if (combat.jumpAttack.poise <= 0f) combat.jumpAttack.poise = JumpPoise;

            // Blocking costs stamina in the test scenes only; Prototype keeps the unlimited guard.
            PlayerDefence defence = combat.GetComponent<PlayerDefence>();
            if (defence != null && defence.blockStaminaCost <= 0f)
            {
                defence.blockStaminaCost = TestBlockStaminaCost;
                EditorUtility.SetDirty(defence);
            }
            // Same for hit stun: a hit that lands interrupts the player, in the test scenes only.
            if (defence != null && defence.hitStunDuration <= 0f)
            {
                defence.hitStunDuration = TestHitStunDuration;
                EditorUtility.SetDirty(defence);
            }

            // Parries stagger the test dummies so the player can riposte. Prototype's dummies don't.
            foreach (DummyAttacker dummy in scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<DummyAttacker>(true)))
            {
                if (dummy.staggerDuration > 0f) continue;
                dummy.staggerDuration = TestStaggerDuration;
                EditorUtility.SetDirty(dummy);
            }

            // Lock-on, in the test scenes only for now.
            if (combat.GetComponent<PlayerLockOn>() == null)
                combat.gameObject.AddComponent<PlayerLockOn>();
            // Souls-style enemy health bars.
            if (combat.GetComponent<EnemyHealthBars>() == null)
                combat.gameObject.AddComponent<EnemyHealthBars>();

            // One Dark Ranger a little way in front of the player, if the scene has no enemy yet.
            bool hasEnemy = scene.GetRootGameObjects().Any(root => root.GetComponentInChildren<EnemyMelee>(true) != null);
            if (!hasEnemy)
            {
                GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, scene);
                Transform player = combat.transform;
                enemy.transform.position = player.position + player.forward * EnemyStartDistance;
                enemy.transform.rotation = Quaternion.LookRotation(-player.forward, Vector3.up);
                EnemyMelee melee = enemy.GetComponent<EnemyMelee>();
                melee.player = combat.GetComponent<PlayerStats>();
                EditorUtility.SetDirty(melee);
            }

            EditorUtility.SetDirty(combat);
            EditorUtility.SetDirty(animation);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
        }
    }

    // ------------------------------------------------------------------------
    // Dark Ranger enemy
    // ------------------------------------------------------------------------

    private const string EnemyPrefabPath = "Assets/Prefabs/Combat/DarkRanger.prefab";
    private const string EnemyModelPath = "Assets/Art/Characters/Player/PlayerRangerMale.prefab";
    private const string EnemyWeaponPath = "Assets/ThirdParty/Quaternius/MedievalWeapons/Models/Sword.fbx";
    private const float EnemyStartDistance = 14f;

    // The player's Ranger, darkened, with the player's controller and a sword. Built once;
    // after that, changes made to the prefab are kept. Delete the prefab to rebuild it.
    private static GameObject GetOrCreateDarkRanger(AnimationClip quick, AnimationClip followUp,
        AnimationClip overhead, AnimationClip delayed)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
        if (existing != null) return existing;

        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyModelPath);
        if (modelPrefab == null) throw new InvalidOperationException("Missing " + EnemyModelPath);
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);

        GameObject root = new GameObject("DarkRanger");
        try
        {
            // Same capsule and model placement as the Player in the test scenes.
            CharacterController capsule = root.AddComponent<CharacterController>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = Vector3.zero;
            root.AddComponent<Health>().maxHealth = 250;
            root.AddComponent<DamageFlash>();

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, root.transform);
            model.name = "DarkRangerModel";
            model.transform.localPosition = new Vector3(0f, -0.892421f, 0f);
            model.transform.localScale = Vector3.one * 1.0124681f;
            Animator animator = model.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            EnemyMelee enemy = root.AddComponent<EnemyMelee>();
            enemy.characterAnimator = animator;
            enemy.weaponModel = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyWeaponPath);
            enemy.attacks = new[]
            {
                // 0: quick slash that may carry on into a second slash.
                EnemyMove("Quick slash", quick, 0.42f, 12f, 2.3f, 0.6f, 0.5f, followUp: 1),
                // 1: the follow-up, also used on its own.
                EnemyMove("Backhand slash", followUp, 0.42f, 12f, 2.3f, 0.4f, 0.7f),
                // 2: slow, hard-hitting overhead that lunges in.
                EnemyMove("Overhead smash", overhead, 0.35f, 22f, 2.6f, 1.2f, 1f, track: 0.7f),
                // 3: holds its wind-up, so rolling on reflex gets punished.
                EnemyMove("Delayed slash", delayed, 0.4f, 18f, 2.5f, 0.8f, 0.9f, track: 1f, hold: 0.6f),
            };

            return PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    // hitPoint is how far through the clip the blade connects; track is the share of the
    // wind-up the enemy keeps turning; hold is how long a delayed swing freezes at the top.
    private static EnemyAttack EnemyMove(string label, AnimationClip clip, float hitPoint, float damage, float reach,
        float lunge, float recovery, float track = 0.6f, float hold = 0f, int followUp = -1)
    {
        float hitTime = clip.length * hitPoint;
        return new EnemyAttack
        {
            label = label,
            state = ShortName(clip),
            damage = damage,
            reach = reach,
            hitTime = hitTime,
            duration = clip.length * 0.85f,
            trackUntil = hitTime * track,
            lunge = lunge,
            holdAt = hold > 0f ? hitTime * 0.6f : 0f,
            holdFor = hold,
            recovery = recovery,
            followUp = followUp,
            followUpChance = 0.5f,
        };
    }
}
