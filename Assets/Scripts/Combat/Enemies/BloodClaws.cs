using UnityEngine;

// The vampire's claws: a faint red glow on each hand, and a red trail from the fingertips while
// it attacks, so its swipes are easy to read in the dark. Goes on an enemy with EnemyMelee and a
// Humanoid model. Flares up when the enemy enters its second phase.
[RequireComponent(typeof(EnemyMelee))]
public class BloodClaws : MonoBehaviour
{
    [Tooltip("Material for the trails. An additive particle material works best.")]
    public Material trailMaterial;
    public Color colour = new Color(0.75f, 0.02f, 0.02f);
    [Tooltip("How long a trail lingers, in seconds.")]
    [Min(0.02f)] public float trailTime = 0.22f;
    [Min(0.005f)] public float trailWidth = 0.12f;

    [Header("Glow")]
    [Tooltip("A small red light on each hand.")]
    public bool handLights = true;
    [Min(0f)] public float lightIntensity = 1.2f;
    [Min(0.1f)] public float lightRange = 1.6f;
    [Tooltip("Glow and trail are multiplied by this in the second phase.")]
    [Min(1f)] public float secondPhaseFlare = 1.8f;

    private EnemyMelee enemy;
    private TrailRenderer[] trails = new TrailRenderer[0];
    private Light[] lights = new Light[0];
    private float flare = 1f;

    private void Awake()
    {
        enemy = GetComponent<EnemyMelee>();
        enemy.EnteredSecondPhase += Flare;
    }

    private void OnDestroy()
    {
        if (enemy != null) enemy.EnteredSecondPhase -= Flare;
    }

    private void Start()
    {
        Animator animator = enemy.characterAnimator;
        if (animator == null || !animator.isHuman) return;
        trails = new[]
        {
            MakeClaw(animator, HumanBodyBones.LeftMiddleDistal, HumanBodyBones.LeftHand, out Light left),
            MakeClaw(animator, HumanBodyBones.RightMiddleDistal, HumanBodyBones.RightHand, out Light right),
        };
        lights = new[] { left, right };
    }

    // On the middle fingertip if the model has one, otherwise the hand.
    private TrailRenderer MakeClaw(Animator animator, HumanBodyBones tip, HumanBodyBones hand, out Light glow)
    {
        Transform bone = animator.GetBoneTransform(tip);
        if (bone == null) bone = animator.GetBoneTransform(hand);
        glow = null;
        if (bone == null) return null;

        GameObject claw = new GameObject("BloodClaw");
        claw.transform.SetParent(bone, false);

        TrailRenderer trail = claw.AddComponent<TrailRenderer>();
        trail.sharedMaterial = trailMaterial;
        trail.time = trailTime;
        trail.minVertexDistance = 0.02f;
        trail.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        trail.widthMultiplier = trailWidth;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.emitting = false;
        SetTrailColour(trail);

        if (handLights)
        {
            glow = claw.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = colour;
            glow.range = lightRange;
            glow.intensity = lightIntensity;
            glow.shadows = LightShadows.None;
        }
        return trail;
    }

    private void SetTrailColour(TrailRenderer trail)
    {
        Gradient fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(colour * 1.2f, 0f), new GradientColorKey(colour * 0.4f, 1f) },
            new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
        trail.colorGradient = fade;
    }

    private void Update()
    {
        bool attacking = enemy.IsAttacking;
        foreach (TrailRenderer trail in trails)
            if (trail != null) trail.emitting = attacking;

        // A slow pulse, brighter while attacking.
        float pulse = (0.8f + 0.2f * Mathf.Sin(Time.time * 3f)) * (attacking ? 1.6f : 1f) * flare;
        foreach (Light glow in lights)
            if (glow != null) glow.intensity = lightIntensity * pulse;
    }

    private void Flare()
    {
        flare = secondPhaseFlare;
        foreach (TrailRenderer trail in trails)
            if (trail != null) trail.widthMultiplier = trailWidth * secondPhaseFlare;
    }
}
