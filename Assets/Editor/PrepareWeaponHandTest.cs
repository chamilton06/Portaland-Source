using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Puts the Quaternius sword and axe in the ranger's hand in the two test scenes only.
// Prototype and the shared Sword/Axe items and prefabs aren't changed.
public static class PrepareWeaponHandTest
{
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Tests/RangerMovementTest.unity",
        "Assets/Scenes/Tests/RangerMovementTestFemale.unity",
    };
    private const string Models = "Assets/ThirdParty/Quaternius/MedievalWeapons/Models/";

    [MenuItem("Tools/Portaland/Prepare Weapon Hand Test")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        try
        {
            Weapon sword = Load<Weapon>("Assets/Items/Sword.asset");
            Weapon axe = Load<Weapon>("Assets/Items/Axe.asset");
            GameObject swordModel = Load<GameObject>(Models + "Sword.fbx");
            GameObject axeModel = Load<GameObject>(Models + "Axe.fbx");

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

                WeaponHandSocket socket = combat.GetComponent<WeaponHandSocket>();
                if (socket == null) socket = combat.gameObject.AddComponent<WeaponHandSocket>();
                PlayerLocomotionAnimation locomotion = combat.GetComponent<PlayerLocomotionAnimation>();
                if (socket.characterAnimator == null && locomotion != null)
                    socket.characterAnimator = locomotion.characterAnimator;

                // Leave existing entries alone so re-running doesn't undo any grip tuning.
                if (combat.heldWeaponModels == null || combat.heldWeaponModels.Length == 0)
                {
                    combat.heldWeaponModels = new[]
                    {
                        new HeldWeaponModel { weapon = sword, model = swordModel, length = 1f, gripPoint = 0.08f },
                        new HeldWeaponModel { weapon = axe, model = axeModel, length = 0.9f, gripPoint = 0.12f },
                    };
                }

                EditorUtility.SetDirty(combat);
                EditorUtility.SetDirty(socket);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
            }

            Debug.Log("Weapon hand test ready in both Ranger movement test scenes. Open one, press Play and tune Grip Position/Rotation on Weapon Hand Socket.");
        }
        catch (Exception error)
        {
            Debug.LogError("Weapon hand test setup stopped: " + error.Message);
        }
    }

    private static T Load<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new InvalidOperationException("Missing " + path);
        return asset;
    }
}
