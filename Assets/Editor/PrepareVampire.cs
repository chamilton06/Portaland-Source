using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Sets up the Sketchfab vampire: Humanoid import, materials, and a Vampire enemy prefab
// built the same way as the Dark Ranger. The prefab isn't placed in any scene.
public static class PrepareVampire
{
    private const string Folder = "Assets/ThirdParty/Sketchfab/Vampire/";
    private const string ModelPath = Folder + "Model/Vampire.fbx";
    private const string TexturePath = Folder + "Textures/";
    private const string MaterialFolder = "Assets/Art/Characters/Vampire/Materials";
    private const string PrefabPath = "Assets/Prefabs/Combat/Vampire.prefab";
    private const string DarkRangerPath = "Assets/Prefabs/Combat/DarkRanger.prefab";
    private const string ControllerPath = "Assets/Animations/Player/RangerDirectional.controller";
    private const float CapsuleHeight = 2f;
    private const float ModelHeight = 1.9f;

    // Which textures go on which part. Matched against the material and mesh names.
    private static readonly (string[] keys, string colour, string normal)[] Parts =
    {
        (new[] { "hair", "haris" }, null, "haris_low_Material.006_Normal"),
        (new[] { "hand", "008" }, "hands_Material.008_BaseColor", "hands_Material.008_Normal"),
        (new[] { "body" }, "Body_bODY_BaseColor", "Body_Normal"),
        (new[] { "vampire", "face", "head", "004" }, "LowpolyVampire_vampire_BaseColor", "LowpolyVampire_vampire_Normal"),
    };

    [MenuItem("Tools/Portaland/Prepare Vampire")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        try
        {
            ModelImporter importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Missing " + ModelPath);

            MarkNormalMaps();
            ImportAsHumanoid(importer);
            CreateMaterials(importer);
            GameObject prefab = GetOrCreatePrefab();
            KeepVisible();
            UseRescaledMesh();
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log("Vampire ready: " + PrefabPath + ". Drag it into a test scene to try it.");
        }
        catch (Exception error)
        {
            Debug.LogError("Vampire setup stopped: " + error.Message);
        }
    }

    // Placing the vampire by standing where he should go. In Play mode, walk to the spot
    // and run "Mark Vampire Spot Here": he's moved there straight away so you can check it.
    // Play mode changes are thrown away when Play stops, so the spot is remembered and
    // applied to the scene automatically as soon as Play stops (see VampireSpotOnStop).
    private const string SpotKey = "Portaland.VampireSpot";
    internal const string PendingKey = "Portaland.VampireSpotPending";

    [MenuItem("Tools/Portaland/Mark Vampire Spot Here")]
    public static void MarkSpot()
    {
        PlayerStats player = UnityEngine.Object.FindFirstObjectByType<PlayerStats>();
        if (!EditorApplication.isPlaying || player == null)
        {
            Debug.LogError("Press Play, walk to where the vampire should stand, then run this.");
            return;
        }
        // A couple of metres in front of the player, at the player's own height so it's on the same floor.
        Transform p = player.transform;
        Vector3 forward = Vector3.ProjectOnPlane(p.forward, Vector3.up).normalized;
        Vector3 spot = p.position + forward * 2.5f;
        float facing = Quaternion.LookRotation(-forward, Vector3.up).eulerAngles.y;
        EditorPrefs.SetString(SpotKey, string.Join(";", new[] { spot.x, spot.y, spot.z, facing }.Select(f => f.ToString(System.Globalization.CultureInfo.InvariantCulture))));

        EnemyMelee vampire = FindVampire();
        if (vampire != null)
        {
            CharacterController capsule = vampire.GetComponent<CharacterController>();
            if (capsule != null) capsule.enabled = false; // let the teleport through
            vampire.transform.SetPositionAndRotation(spot + Vector3.up * 0.2f, Quaternion.Euler(0f, facing, 0f));
            if (capsule != null) capsule.enabled = true;
        }
        EditorPrefs.SetBool(PendingKey, true);
        Debug.Log("Vampire spot marked at " + spot + ". When you stop Play he'll be moved there in the scene. Then save.");
    }

    [MenuItem("Tools/Portaland/Place Vampire At Marked Spot")]
    public static void PlaceAtSpot()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Stop Play first, then run this.");
            return;
        }
        string[] parts = EditorPrefs.GetString(SpotKey, "").Split(';');
        if (parts.Length != 4)
        {
            Debug.LogError("No spot marked yet. Press Play, stand where he should go and run Mark Vampire Spot Here.");
            return;
        }
        float[] v = parts.Select(x => float.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EnemyMelee vampire = FindVampire();
        GameObject placed = vampire != null ? vampire.gameObject
            : (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene);
        Undo.RegisterCompleteObjectUndo(placed.transform, "Place Vampire");
        placed.transform.SetPositionAndRotation(new Vector3(v[0], v[1] + 0.2f, v[2]), Quaternion.Euler(0f, v[3], 0f));
        Selection.activeGameObject = placed;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Vampire placed at the marked spot. Save the scene to keep it.");
    }

    // Makes the vampire in the open scene the boss. Only that one is changed; the prefab stays
    // a normal vampire so it can be reused as a regular enemy. Values are only set the first
    // time, so tuning done in the Inspector afterwards is kept.
    [MenuItem("Tools/Portaland/Make Vampire The Boss")]
    public static void MakeBoss()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Stop Play first, then run this.");
            return;
        }
        EnemyMelee vampire = FindVampire();
        if (vampire == null)
        {
            Debug.LogError("No vampire in this scene. Place one first.");
            return;
        }
        if (vampire.GetComponent<BossEncounter>() != null)
        {
            Debug.Log("The vampire is already the boss. Its settings are on the Boss Encounter component.");
            Selection.activeGameObject = vampire.gameObject;
            return;
        }

        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { vampire, vampire.GetComponent<Health>() }, "Make Vampire The Boss");
        BossEncounter boss = Undo.AddComponent<BossEncounter>(vampire.gameObject);
        boss.bossName = "Servant of Dracula";
        vampire.waitToBeWoken = true;
        vampire.aggroRange = 30f;    // Once awake, he doesn't lose interest inside the church.
        vampire.giveUpRange = 60f;
        vampire.poise = 70f;         // Takes a proper combo to stagger, not two hits.
        Health health = vampire.GetComponent<Health>();
        health.maxHealth = 500;
        vampire.name = "Vampire (Boss)";
        EditorUtility.SetDirty(vampire);
        EditorUtility.SetDirty(health);
        Selection.activeGameObject = vampire.gameObject;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(vampire.gameObject.scene);
        Debug.Log("The vampire is now the boss: 500 health, waits until you're within 10 m on his floor. Save the scene to keep it.");
    }

    private static EnemyMelee FindVampire()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        return UnityEngine.Object.FindObjectsByType<EnemyMelee>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(e => PrefabUtility.GetCorrespondingObjectFromSource(e.gameObject) == prefab || e.name.StartsWith("Vampire"));
    }

    // Prints what the vampire's renderer is doing, so we can see why it isn't drawn.
    // Works in Play mode too.
    [MenuItem("Tools/Portaland/Debug/Report Vampire")]
    public static void Report()
    {
        var log = new System.Text.StringBuilder("VAMPIRE REPORT (playing: " + EditorApplication.isPlaying + ")\n");
        foreach (EnemyMelee enemy in UnityEngine.Object.FindObjectsByType<EnemyMelee>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!enemy.name.Contains("Vampire")) continue;
            log.AppendLine(enemy.name + " active=" + enemy.gameObject.activeInHierarchy + " pos=" + enemy.transform.position);
            Animator animator = enemy.characterAnimator;
            if (animator != null)
            {
                log.AppendLine(" animator: enabled=" + animator.enabled + " human=" + animator.isHuman + " avatarValid=" + (animator.avatar != null && animator.avatar.isValid)
                    + " controller=" + (animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "none") + " culling=" + animator.cullingMode);
                Transform hips = animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Hips) : null;
                if (hips != null) log.AppendLine(" hips: pos=" + hips.position + " lossyScale=" + hips.lossyScale);
                Transform head = animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
                if (head != null) log.AppendLine(" head: pos=" + head.position);
                log.AppendLine(" model: localPos=" + animator.transform.localPosition + " lossyScale=" + animator.transform.lossyScale + " active=" + animator.gameObject.activeInHierarchy);
            }
            foreach (Renderer r in enemy.GetComponentsInChildren<Renderer>(true))
            {
                log.AppendLine(" renderer " + r.name + " (" + r.GetType().Name + "): enabled=" + r.enabled + " active=" + r.gameObject.activeInHierarchy
                    + " visible=" + r.isVisible + " bounds=" + r.bounds.center + " size=" + r.bounds.size
                    + " lossyScale=" + r.transform.lossyScale + " layer=" + LayerMask.LayerToName(r.gameObject.layer)
                    + " materials=" + string.Join(",", r.sharedMaterials.Select(m => m == null ? "NULL" : m.name + "/" + m.shader.name)));
                if (r is SkinnedMeshRenderer skin)
                {
                    Mesh mesh = skin.sharedMesh;
                    log.AppendLine("   mesh=" + (mesh == null ? "NULL" : mesh.name + " verts=" + mesh.vertexCount + " meshBounds=" + mesh.bounds.size + " subMeshes=" + mesh.subMeshCount)
                        + " bones=" + skin.bones.Length + " nullBones=" + skin.bones.Count(b => b == null)
                        + " rootBone=" + (skin.rootBone != null ? skin.rootBone.name : "none") + " offscreen=" + skin.updateWhenOffscreen
                        + " quality=" + skin.quality + " localBounds=" + skin.localBounds.size);
                    if (mesh != null && mesh.vertexCount > 0)
                    {
                        var baked = new Mesh();
                        skin.BakeMesh(baked, true);
                        log.AppendLine("   baked (skinned now) bounds centre=" + skin.transform.TransformPoint(baked.bounds.center) + " size=" + baked.bounds.size);
                        UnityEngine.Object.DestroyImmediate(baked);
                    }
                }
            }
            Camera cam = Camera.main;
            if (cam != null) log.AppendLine(" camera " + cam.name + " pos=" + cam.transform.position + " cullingMask has Default=" + ((cam.cullingMask & 1) != 0) + " near=" + cam.nearClipPlane + " far=" + cam.farClipPlane);
        }
        Debug.Log(log.ToString());
    }

    private static void MarkNormalMaps()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { TexturePath.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter texture = AssetImporter.GetAtPath(path) as TextureImporter;
            if (texture == null || !path.Contains("Normal") || texture.textureType == TextureImporterType.NormalMap) continue;
            texture.textureType = TextureImporterType.NormalMap;
            texture.SaveAndReimport();
        }
    }

    private static void ImportAsHumanoid(ModelImporter importer)
    {
        bool changed = false;
        if (importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            changed = true;
        }
        // The download has a Mixamo take in it. We use our own animations.
        if (importer.importAnimation) { importer.importAnimation = false; changed = true; }
        if (changed) importer.SaveAndReimport();

        Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Avatar>().FirstOrDefault();
        if (avatar == null || !avatar.isHuman || !avatar.isValid)
            throw new InvalidOperationException("Unity couldn't make a Humanoid avatar from " + ModelPath + ". Check its Rig tab.");
    }

    // Our own Standard materials, remapped onto the model so it looks right everywhere.
    private static void CreateMaterials(ModelImporter importer)
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art/Characters/Vampire"))
                AssetDatabase.CreateFolder("Assets/Art/Characters", "Vampire");
            AssetDatabase.CreateFolder("Assets/Art/Characters/Vampire", "Materials");
        }

        bool changed = false;
        // Tidy up after the first version of this tool, which remapped our own materials again.
        // Also drop links to materials that have since been deleted, so they get rebuilt.
        foreach (var entry in importer.GetExternalObjectMap().Where(e => e.Key.name.StartsWith("Vampire_") || e.Value == null).ToArray())
        {
            importer.RemoveRemap(entry.Key);
            changed = true;
        }
        if (changed) { importer.SaveAndReimport(); changed = false; }
        foreach (string guid in AssetDatabase.FindAssets("Vampire_Vampire_ t:Material", new[] { MaterialFolder }))
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
        // Materials already pointing at ours were remapped by an earlier run, so skip those.
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        var slots = model.GetComponentsInChildren<Renderer>(true)
            .SelectMany(r => r.sharedMaterials
                .Where(m => m != null && !AssetDatabase.GetAssetPath(m).StartsWith(MaterialFolder))
                .Select(m => (renderer: r.name, material: m.name)))
            .GroupBy(slot => slot.material).Select(g => g.First()).ToArray();

        foreach (var slot in slots)
        {
            string path = MaterialFolder + "/Vampire_" + Safe(slot.material) + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard")) { name = "Vampire_" + slot.material };
                var part = Parts.FirstOrDefault(p => p.keys.Any(k =>
                    slot.material.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    slot.renderer.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0));
                if (slot.material.IndexOf("eye", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // The eyes have no texture. Red with a faint glow, so they show in the dark church.
                    material.color = new Color(0.6f, 0.02f, 0.02f);
                    material.SetColor("_EmissionColor", new Color(0.9f, 0.05f, 0.03f));
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                }
                else if (part.keys == null)
                    Debug.LogWarning("Vampire: no textures matched material '" + slot.material + "' on '" + slot.renderer + "'. Assign them by hand on " + path);
                else
                {
                    Texture2D colour = part.colour == null ? null : LoadTexture(part.colour);
                    Texture2D normal = LoadTexture(part.normal);
                    if (colour != null) material.mainTexture = colour;
                    else material.color = new Color(0.08f, 0.07f, 0.07f); // hair only came with a normal map
                    if (normal != null)
                    {
                        material.SetTexture("_BumpMap", normal);
                        material.EnableKeyword("_NORMALMAP");
                    }
                }
                material.SetFloat("_Glossiness", 0.3f);
                AssetDatabase.CreateAsset(material, path);
            }
            var id = new AssetImporter.SourceAssetIdentifier(typeof(Material), slot.material);
            if (importer.GetExternalObjectMap().TryGetValue(id, out UnityEngine.Object current) && current == material) continue;
            importer.AddRemap(id, material);
            changed = true;
        }
        if (changed) importer.SaveAndReimport();
    }

    private static GameObject GetOrCreatePrefab()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (existing != null) return existing;

        GameObject darkRanger = AssetDatabase.LoadAssetAtPath<GameObject>(DarkRangerPath);
        EnemyMelee template = darkRanger != null ? darkRanger.GetComponent<EnemyMelee>() : null;
        if (template == null) throw new InvalidOperationException("Run Prepare Combat Animations first. The vampire copies the Dark Ranger's moveset to start with.");

        GameObject root = new GameObject("Vampire");
        try
        {
            CharacterController capsule = root.AddComponent<CharacterController>();
            capsule.height = CapsuleHeight;
            capsule.radius = 0.5f;
            capsule.center = Vector3.zero;
            root.AddComponent<Health>().maxHealth = 250;
            root.AddComponent<DamageFlash>();

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath), root.transform);
            model.name = "VampireModel";
            FitToCapsule(model);
            Animator animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Avatar>().First();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            animator.applyRootMotion = false;

            // Starts as a copy of the Dark Ranger, bare-handed so the swings read as claw
            // swipes. The vampire gets its own moveset next.
            EnemyMelee enemy = root.AddComponent<EnemyMelee>();
            EditorUtility.CopySerialized(template, enemy);
            enemy.characterAnimator = animator;
            enemy.player = null;
            enemy.weaponModel = null;
            enemy.tint = Color.white;

            return PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    // The download is one skinned mesh scaled down to 0.0002 with the skeleton scaling it back
    // up, which leaves Unity's on-screen box in the wrong place, so it culls him while he's
    // right in front of the camera. Recalculating the box every frame fixes that.
    private static void KeepVisible()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            bool changed = false;
            foreach (SkinnedMeshRenderer skin in contents.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (skin.updateWhenOffscreen) continue;
                skin.updateWhenOffscreen = true;
                changed = true;
            }
            if (changed) PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    // The download's mesh is modelled about 842,000 units tall and shrunk by its transform,
    // which breaks skinning and culling in Play mode. This saves a copy with the points scaled
    // to about 2 units and the bind poses adjusted to match, so it skins exactly as before.
    private const string MeshPath = "Assets/Art/Characters/Vampire/VampireMesh.asset";

    private static void UseRescaledMesh()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            bool changed = false;
            foreach (SkinnedMeshRenderer skin in contents.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Mesh source = skin.sharedMesh;
                if (source == null || AssetDatabase.GetAssetPath(source) == MeshPath) continue;

                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
                if (mesh == null)
                {
                    float k = 2f / Mathf.Max(source.bounds.size.y, 0.0001f);
                    mesh = UnityEngine.Object.Instantiate(source);
                    mesh.name = "VampireMesh";
                    mesh.vertices = source.vertices.Select(v => v * k).ToArray();
                    // bindpose' * (k * v) has to equal bindpose * v.
                    Matrix4x4 undo = Matrix4x4.Scale(Vector3.one / k);
                    mesh.bindposes = source.bindposes.Select(b => b * undo).ToArray();
                    if (source.blendShapeCount > 0)
                    {
                        mesh.ClearBlendShapes();
                        var deltas = new Vector3[source.vertexCount];
                        var normals = new Vector3[source.vertexCount];
                        var tangents = new Vector3[source.vertexCount];
                        for (int s = 0; s < source.blendShapeCount; s++)
                            for (int f = 0; f < source.GetBlendShapeFrameCount(s); f++)
                            {
                                source.GetBlendShapeFrameVertices(s, f, deltas, normals, tangents);
                                mesh.AddBlendShapeFrame(source.GetBlendShapeName(s), source.GetBlendShapeFrameWeight(s, f),
                                    deltas.Select(d => d * k).ToArray(), normals, tangents);
                            }
                    }
                    mesh.RecalculateBounds();
                    AssetDatabase.CreateAsset(mesh, MeshPath);
                    Debug.Log("Vampire: saved a rescaled copy of the mesh (x" + k + ") to " + MeshPath);
                }
                skin.sharedMesh = mesh;
                changed = true;
            }
            if (changed) PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    // Scales the model to ModelHeight and stands its feet on the bottom of the capsule.
    private static void FitToCapsule(GameObject model)
    {
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;
        Bounds bounds = MeasureBounds(model);
        if (bounds.size.y < 0.01f) return;
        float scale = ModelHeight / bounds.size.y;
        model.transform.localScale = Vector3.one * scale;
        bounds = MeasureBounds(model);
        float feet = bounds.min.y - model.transform.parent.position.y;
        model.transform.localPosition = new Vector3(0f, -CapsuleHeight * 0.5f - feet, 0f);
    }

    private static Bounds MeasureBounds(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    private static Texture2D LoadTexture(string name) =>
        AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + name + ".png");

    private static string Safe(string name) =>
        new string(name.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
}

// Applies a spot marked in Play mode as soon as Play stops, so it isn't lost.
[InitializeOnLoad]
static class VampireSpotOnStop
{
    static VampireSpotOnStop()
    {
        EditorApplication.playModeStateChanged += change =>
        {
            if (change != PlayModeStateChange.EnteredEditMode || !EditorPrefs.GetBool(PrepareVampire.PendingKey)) return;
            EditorPrefs.SetBool(PrepareVampire.PendingKey, false);
            PrepareVampire.PlaceAtSpot();
        };
    }
}
