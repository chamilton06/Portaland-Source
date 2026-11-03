using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Keep the first gameplay test in its own scene so the group's Prototype stays intact.
[InitializeOnLoad]
public static class PrepareRangerMovementTest
{
    private const string ScenePath = "Assets/Scenes/Tests/RangerMovementTest.unity";
    private const string ControllerPath = "Assets/Animations/Player/RangerLocomotion.controller";

    static PrepareRangerMovementTest()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool("Portaland.MovementTest.Attempted", false)) return;
            SessionState.SetBool("Portaland.MovementTest.Attempted", true);
            Prepare();
        };
    }

    [MenuItem("Tools/Portaland/Prepare Ranger Movement Test")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
        {
            Debug.Log("Ranger movement test already exists: " + ScenePath + ". Keeping your saved changes.");
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
            copiedScene = AssetDatabase.CopyAsset("Assets/Scenes/Prototype.unity", ScenePath);
            if (!copiedScene) throw new InvalidOperationException("Couldn't copy the saved Prototype scene.");
            test = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(test);
            PlayerMovement[] players = test.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PlayerMovement>(true)).ToArray();
            if (players.Length != 1) throw new InvalidOperationException("Expected one gameplay Player in Prototype; found " + players.Length);
            PlayerMovement movement = players[0];
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
            Debug.Log("Ranger movement test ready: " + ScenePath + ". Open it and test WASD, mouse and Shift. Airborne and crouch animations aren't finished yet.");
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
