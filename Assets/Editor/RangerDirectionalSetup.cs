using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Only builds when real directional clips have been supplied. No changes on script reload.
public class RangerDirectionalSetup : EditorWindow
{
    private const string Source = "Assets/Animations/Player/RangerLocomotion.controller";
    private const string Output = "Assets/Animations/Player/RangerDirectional.controller";
    private const string ClipSettings = "Assets/Animations/Player/RangerDirectionalClips.asset";
    private PlayerDirectionalClips clips;
    private Editor settingsEditor;
    private string message;

    [MenuItem("Tools/Portaland/Set Up Directional Animations")]
    public static void Open() => GetWindow<RangerDirectionalSetup>("Ranger Directions");

    void OnEnable() { clips = AssetDatabase.LoadAssetAtPath<PlayerDirectionalClips>(ClipSettings); }
    void OnDisable() { if (settingsEditor != null) DestroyImmediate(settingsEditor); }

    void OnGUI()
    {
        EditorGUILayout.HelpBox("Assign real Humanoid strafe/backwards clips. The current free Standard pack doesn't contain them. The working movement controller stays available.", MessageType.Info);
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
