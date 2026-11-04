using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// This only runs in the Editor. The downloaded models stay unchanged.
public static class PrepareRangerCharacters
{
    private const string Source = "Assets/ThirdParty/Quaternius/";
    private const string Output = "Assets/Art/Characters/Player";

    [MenuItem("Tools/Portaland/Prepare Ranger Characters")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        try
        {
            MakeFolder(Output);
            Build("Male");
            Build("Female");
            AssetDatabase.SaveAssets();
            Debug.Log("Ranger preparation finished. Prefabs are in " + Output + ". Preview them before using them on Player.");
        }
        catch (Exception error)
        {
            Debug.LogError("Ranger preparation stopped without changing the scene: " + error);
        }
    }

    private static void Build(string sex)
    {
        string prefabPath = Output + "/PlayerRanger" + sex + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;
        GameObject bodyAsset = AssetDatabase.LoadAssetAtPath<GameObject>(Source + "BaseCharacters/Models/Superhero_" + sex + "_FullBody.fbx");
        GameObject outfitAsset = AssetDatabase.LoadAssetAtPath<GameObject>(Source + "FantasyOutfits/Models/Outfits/" + sex + "_Ranger.fbx");
        if (bodyAsset == null || outfitAsset == null) throw new InvalidOperationException("Missing source model for " + sex);

        GameObject root = Object.Instantiate(bodyAsset);
        root.name = "PlayerRanger" + sex;
        root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        var createdMeshes = new List<string>();
        try
        {
            Animator animator = root.GetComponent<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                throw new InvalidOperationException(sex + " base model needs a valid Humanoid Avatar first.");
            animator.applyRootMotion = false;
            var bones = new Dictionary<string, Transform>();
            foreach (Transform bone in root.GetComponentsInChildren<Transform>(true))
                if (!bones.ContainsKey(bone.name)) bones.Add(bone.name, bone);

            Transform neck = animator.GetBoneTransform(HumanBodyBones.Neck);
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (head == null) throw new InvalidOperationException("No mapped head on " + sex);
            Transform cutRoot = neck != null ? neck : head;
            int keptTriangles = 0;
            foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Mesh original = renderer.sharedMesh;
                if (original == null) continue;
                BoneWeight[] weights = original.boneWeights;
                if (weights.Length != original.vertexCount) throw new InvalidOperationException("Unexpected skin weights on " + renderer.name);
                bool[] keep = weights.Select(w => HeadWeight(w, renderer.bones, cutRoot) >= 0.5f).ToArray();
                Mesh mesh = Object.Instantiate(original);
                mesh.name = sex + "_Head_" + renderer.name;
                int triangleCount = 0;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    int[] triangles = original.GetTriangles(sub);
                    var selected = new List<int>();
                    for (int i = 0; i < triangles.Length; i += 3)
                        if (keep[triangles[i]] && keep[triangles[i + 1]] && keep[triangles[i + 2]])
                        { selected.Add(triangles[i]); selected.Add(triangles[i + 1]); selected.Add(triangles[i + 2]); }
                    triangleCount += selected.Count / 3;
                    mesh.SetTriangles(selected, sub);
                }
                if (triangleCount == 0) { Object.DestroyImmediate(mesh); renderer.enabled = false; continue; }
                string path = AssetDatabase.GenerateUniqueAssetPath(Output + "/" + mesh.name.Replace('/', '_') + ".asset");
                AssetDatabase.CreateAsset(mesh, path);
                createdMeshes.Add(path);
                renderer.sharedMesh = mesh;
                keptTriangles += triangleCount;
            }
            if (keptTriangles == 0) throw new InvalidOperationException("Head extraction produced no triangles.");

            GameObject outfit = Object.Instantiate(outfitAsset, root.transform, false);
            outfit.name = "RangerOutfit";
            foreach (Animator extra in outfit.GetComponentsInChildren<Animator>(true)) Object.DestroyImmediate(extra);
            foreach (SkinnedMeshRenderer renderer in outfit.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Transform[] mapped = renderer.bones.Select(b => b != null && bones.TryGetValue(b.name, out Transform target)
                    ? target : throw new InvalidOperationException("Cannot match outfit bone " + (b == null ? "null" : b.name))).ToArray();
                Mesh mesh = Object.Instantiate(renderer.sharedMesh);
                mesh.name = sex + "_Outfit_" + renderer.name;
                // Rebuild bind poses so the clothing uses the base character's skeleton.
                mesh.bindposes = mapped.Select(b => b.worldToLocalMatrix * renderer.transform.localToWorldMatrix).ToArray();
                string path = AssetDatabase.GenerateUniqueAssetPath(Output + "/" + mesh.name.Replace('/', '_') + ".asset");
                AssetDatabase.CreateAsset(mesh, path);
                createdMeshes.Add(path);
                renderer.sharedMesh = mesh;
                renderer.bones = mapped;
                if (renderer.rootBone != null && bones.TryGetValue(renderer.rootBone.name, out Transform mappedRoot)) renderer.rootBone = mappedRoot;
            }
            if (PrefabUtility.SaveAsPrefabAsset(root, prefabPath) == null) throw new InvalidOperationException("Could not save " + prefabPath);
        }
        catch
        {
            foreach (string path in createdMeshes) AssetDatabase.DeleteAsset(path);
            throw;
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static float HeadWeight(BoneWeight weight, Transform[] bones, Transform head)
    {
        return Part(weight.boneIndex0, weight.weight0) + Part(weight.boneIndex1, weight.weight1)
            + Part(weight.boneIndex2, weight.weight2) + Part(weight.boneIndex3, weight.weight3);
        float Part(int index, float value) => index >= 0 && index < bones.Length && bones[index] != null
            && (bones[index] == head || bones[index].IsChildOf(head)) ? value : 0f;
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
