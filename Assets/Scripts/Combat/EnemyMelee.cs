using UnityEngine;

// A sword-fighting enemy in the Elden Ring style. It chases the player, circles at a distance
// while it picks an attack, then closes in and commits. Its attacks are telegraphed, can be
// blocked, parried or rolled through, and leave it open afterwards.
//  - Parry its strike, or break its poise with enough hits, and it staggers: hit it then for a riposte.
//  - It flinches when hit while it isn't attacking, but keeps swinging through hits mid-attack.
// Uses the same Animator controller as the player (Speed/MoveX/MoveZ for walking, Combat and Death layers).
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CharacterController))]
public class EnemyMelee : MonoBehaviour, IStaggerable, IHitReceiver
{
    [Tooltip("Found automatically if left empty.")]
    public PlayerStats player;
    [Tooltip("The character model's Animator. Found automatically if left empty.")]
    public Animator characterAnimator;

    [Header("Senses")]
    [Tooltip("The enemy notices the player inside this distance.")]
    [Min(1f)] public float aggroRange = 12f;
    [Tooltip("It gives up and stands still if the player gets further than this.")]
    [Min(1f)] public float giveUpRange = 25f;

    [Header("Movement")]
    [Min(0.1f)] public float runSpeed = 4f;
    [Tooltip("Speed while circling the player.")]
    [Min(0.1f)] public float strafeSpeed = 1.6f;
    [Min(1f)] public float turnSpeed = 360f;
    [Tooltip("Distance it keeps while circling and choosing an attack.")]
    [Min(1f)] public float circleDistance = 3.5f;
    [Tooltip("Shortest and longest time it circles before attacking.")]
    public Vector2 circleTime = new Vector2(0.8f, 2.2f);
    [Min(0f)] public float gravity = 20f;

    [Header("Moveset")]
    public EnemyAttack[] attacks;
    [Range(1f, 180f)] public float attackAngle = 100f;

    [Header("Poise and stagger")]
    [Tooltip("Damage it can take (within Poise Recover Delay of each hit) before its poise breaks and it staggers.")]
    [Min(1f)] public float poise = 45f;
    [Min(0f)] public float poiseRecoverDelay = 3f;
    [Tooltip("Seconds it's staggered after a parry or poise break. Hit it then for a riposte.")]
    [Min(0.1f)] public float staggerDuration = 1.5f;
    [Min(0f)] public float parryPushback = 0.5f;
    [Tooltip("Seconds a flinch interrupts it when hit outside an attack.")]
    [Min(0f)] public float flinchTime = 0.35f;

    [Header("Look")]
    [Tooltip("Sword model to hold in the right hand. Its blade should point along its Y axis.")]
    public GameObject weaponModel;
    [Min(0.05f)] public float weaponLength = 1f;
    [Range(0f, 1f)] public float weaponGripPoint = 0.08f;
    [Tooltip("Multiplies the model's colours. Dark grey gives a shadowy invader look; white leaves it unchanged.")]
    public Color tint = new Color(0.35f, 0.32f, 0.42f);

    public bool logActions = true;

    private enum State { Idle, Chase, Circle, Approach, Attack, Recover, Flinch, Staggered, Dead }
    private State state;
    private float stateEndsAt;
    private EnemyAttack attack;
    private float attackTime;      // Animation seconds into the current attack.
    private float holdEndsAt;
    private bool struck;
    private float circleSide = 1f;
    private float poiseDamage;
    private float lastHitTime = float.NegativeInfinity;
    private float verticalSpeed;
    private Vector3 velocity;

    private CharacterController controller;
    private Health health;
    private PlayerDefence playerDefence;
    private int combatLayer = -1;
    private int deathLayer = -1;

    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveZ = Animator.StringToHash("MoveZ");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int StaggerState = Animator.StringToHash("Idle_Shield_Break");
    private static readonly int HitState = Animator.StringToHash("Hit_Chest");
    private static readonly int DeathState = Animator.StringToHash("Death01");

    public bool IsStaggered => state == State.Staggered;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<Health>();
        health.onDeath.AddListener(Die);
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<Animator>();
        if (characterAnimator != null)
        {
            characterAnimator.applyRootMotion = false;
            combatLayer = characterAnimator.GetLayerIndex("Combat");
            deathLayer = characterAnimator.GetLayerIndex("Death");
            ApplyTint();
        }
        if (combatLayer < 0)
            Debug.LogWarning("EnemyMelee needs the Ranger controller with a Combat layer. Run Tools > Portaland > Prepare Combat Animations.", this);
    }

    private void Start()
    {
        if (player == null) player = FindFirstObjectByType<PlayerStats>();
        if (player != null) playerDefence = player.GetComponent<PlayerDefence>();
        HoldWeapon();
    }

    private void OnDestroy()
    {
        if (health != null) health.onDeath.RemoveListener(Die);
    }

    // ------------------------------------------------------------------------
    // Brain
    // ------------------------------------------------------------------------

    private void Update()
    {
        if (state == State.Dead) return;
        velocity = Vector3.zero;

        if (player == null || player.IsDead())
        {
            if (state != State.Staggered) SetState(State.Idle);
        }
        else
        {
            if (Time.time - lastHitTime > poiseRecoverDelay) poiseDamage = 0f;
            Think();
        }

        Move();
        Animate();
    }

    private void Think()
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        switch (state)
        {
            case State.Idle:
                if (distance < aggroRange) SetState(State.Chase);
                break;

            case State.Chase:
                if (distance > giveUpRange) { SetState(State.Idle); break; }
                Face(toPlayer);
                velocity = Steer(toPlayer.normalized) * runSpeed;
                if (distance <= circleDistance + 0.5f) StartCircling();
                break;

            case State.Circle:
                Face(toPlayer);
                if (distance > circleDistance + 2.5f) { SetState(State.Chase); break; }
                // Side-step around the player while holding the distance.
                Vector3 side = Vector3.Cross(Vector3.up, toPlayer.normalized) * circleSide;
                float closeIn = Mathf.Clamp(distance - circleDistance, -1f, 1f);
                velocity = (side + toPlayer.normalized * closeIn).normalized * strafeSpeed;
                if (Time.time >= stateEndsAt) ChooseAttack();
                break;

            case State.Approach:
                Face(toPlayer);
                if (distance <= attack.reach * 0.85f) StartAttack(attack);
                else if (Time.time >= stateEndsAt) StartCircling();
                else velocity = Steer(toPlayer.normalized) * runSpeed;
                break;

            case State.Attack:
                UpdateAttack(toPlayer, distance);
                break;

            case State.Recover:
            case State.Flinch:
            case State.Staggered:
                if (Time.time >= stateEndsAt) StartCircling();
                break;
        }
    }

    private void StartCircling()
    {
        circleSide = Random.value < 0.5f ? -1f : 1f;
        SetState(State.Circle, Random.Range(circleTime.x, circleTime.y));
    }

    private void ChooseAttack()
    {
        if (attacks == null || attacks.Length == 0) { StartCircling(); return; }
        attack = attacks[Random.Range(0, attacks.Length)];
        SetState(State.Approach, 3f);
    }

    // ------------------------------------------------------------------------
    // Attacks
    // ------------------------------------------------------------------------

    private void StartAttack(EnemyAttack next)
    {
        attack = next;
        attackTime = 0f;
        holdEndsAt = float.NegativeInfinity;
        struck = false;
        SetState(State.Attack);
        PlayCombatState(Animator.StringToHash(attack.state), 0.1f);
        Report(attack.label);
    }

    private void UpdateAttack(Vector3 toPlayer, float distance)
    {
        // A delayed swing freezes at the top of its wind-up for a moment.
        bool holding = attack.holdFor > 0f && attackTime >= attack.holdAt && Time.time < holdEndsAt;
        if (attack.holdFor > 0f && attackTime >= attack.holdAt && holdEndsAt == float.NegativeInfinity)
        {
            holdEndsAt = Time.time + attack.holdFor;
            holding = true;
        }
        if (characterAnimator != null) characterAnimator.speed = holding ? 0.05f : 1f;
        if (!holding) attackTime += Time.deltaTime;

        if (attackTime < attack.trackUntil || holding) Face(toPlayer);

        // Lunge in during the wind-up, but don't walk through the player.
        if (!struck && !holding && attack.hitTime > 0f && distance > 1.2f)
            velocity = transform.forward * (attack.lunge / attack.hitTime);

        if (!struck && attackTime >= attack.hitTime)
        {
            struck = true;
            Strike();
            if (state != State.Attack) return; // A parry staggered it.
        }

        if (attackTime >= attack.duration)
        {
            bool combo = attack.followUp >= 0 && attack.followUp < attacks.Length && Random.value < attack.followUpChance;
            if (combo) StartAttack(attacks[attack.followUp]);
            else SetState(State.Recover, attack.recovery);
        }
    }

    // The blade connects: check it actually reaches the player, then let their defence decide.
    private void Strike()
    {
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f;
        if (direction.magnitude > attack.reach) { Report("Miss: out of range"); return; }
        if (Vector3.Angle(transform.forward, direction) > attackAngle * 0.5f) { Report("Miss: dodged to the side"); return; }

        Vector3 origin = transform.position + Vector3.up * 0.25f;
        Vector3 target = player.transform.position + Vector3.up * 0.25f;
        foreach (RaycastHit hit in Physics.RaycastAll(origin, target - origin, Vector3.Distance(origin, target),
                     Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(transform) || hit.collider.transform.IsChildOf(player.transform)) continue;
            Report("Miss: blocked by " + hit.collider.name);
            return;
        }

        if (playerDefence != null && playerDefence.isActiveAndEnabled)
        {
            PlayerDefence.HitResult result = playerDefence.ReceiveHit(attack.damage, transform.position);
            Report("Strike: " + result);
            if (result == PlayerDefence.HitResult.Parried) Stagger(true);
        }
        else
        {
            player.TakeDamage(attack.damage);
            Report("Strike: Damaged");
        }
    }

    // ------------------------------------------------------------------------
    // Getting hit
    // ------------------------------------------------------------------------

    public void OnHit(int damage)
    {
        if (state == State.Dead || state == State.Staggered) return;
        lastHitTime = Time.time;
        poiseDamage += damage;

        if (poiseDamage >= poise)
        {
            poiseDamage = 0f;
            Report("Poise broken");
            Stagger(false);
        }
        else if (state != State.Attack)
        {
            // Outside an attack every hit makes it flinch; mid-attack it swings on through.
            SetState(State.Flinch, flinchTime);
            PlayCombatState(HitState, 0.05f);
        }
    }

    // Knocked off balance by a parry or poise break: open to a riposte.
    private void Stagger(bool parried)
    {
        SetState(State.Staggered, staggerDuration);
        if (parried && parryPushback > 0f) controller.Move(-transform.forward * parryPushback);
        PlayCombatState(StaggerState, 0.05f);
        Report("Staggered");
    }

    public void EndStagger()
    {
        if (state != State.Staggered) return;
        SetState(State.Recover, 1f);
        PlayCombatState(HitState, 0.05f);
        Report("Riposted");
    }

    private void Die()
    {
        SetState(State.Dead);
        // The body shouldn't block the player or catch their attacks.
        controller.enabled = false;
        if (characterAnimator != null && deathLayer >= 0 && characterAnimator.HasState(deathLayer, DeathState))
            characterAnimator.CrossFadeInFixedTime(DeathState, 0.1f, deathLayer, 0f);
        Report("Died");
    }

    // ------------------------------------------------------------------------
    // Movement and animation
    // ------------------------------------------------------------------------

    private void SetState(State next, float duration = 0f)
    {
        state = next;
        stateEndsAt = Time.time + duration;
        if (characterAnimator != null) characterAnimator.speed = 1f;
    }

    private void Face(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(direction, Vector3.up), turnSpeed * Time.deltaTime);
    }

    // Heads toward the player, swinging round obstacles in the way.
    private Vector3 Steer(Vector3 direction)
    {
        Vector3 origin = transform.position + Vector3.up * 0.3f;
        foreach (float angle in new[] { 0f, 35f, -35f, 70f, -70f })
        {
            Vector3 tryDirection = Quaternion.Euler(0f, angle, 0f) * direction;
            if (!Blocked(origin, tryDirection)) return tryDirection;
        }
        return direction;
    }

    private bool Blocked(Vector3 origin, Vector3 direction)
    {
        foreach (RaycastHit hit in Physics.SphereCastAll(origin, controller.radius * 0.8f, direction, 1.5f,
                     Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(transform)) continue;
            if (player != null && hit.collider.transform.IsChildOf(player.transform)) continue;
            if (hit.normal.y > 0.6f) continue; // Walkable ground, not a wall.
            return true;
        }
        return false;
    }

    private void Move()
    {
        if (!controller.enabled) return;
        verticalSpeed = controller.isGrounded ? -2f : verticalSpeed - gravity * Time.deltaTime;
        controller.Move((velocity + Vector3.up * verticalSpeed) * Time.deltaTime);
    }

    private void Animate()
    {
        if (characterAnimator == null) return;
        Vector3 flat = Vector3.ProjectOnPlane(controller.velocity, Vector3.up);
        Vector3 local = transform.InverseTransformDirection(flat);
        Vector2 direction = Vector2.ClampMagnitude(new Vector2(local.x, local.z) / strafeSpeed, 1f);
        characterAnimator.SetFloat(Speed, flat.magnitude, 0.12f, Time.deltaTime);
        characterAnimator.SetFloat(MoveX, direction.x, 0.12f, Time.deltaTime);
        characterAnimator.SetFloat(MoveZ, direction.y, 0.12f, Time.deltaTime);
        characterAnimator.SetBool(Grounded, true);
    }

    private void PlayCombatState(int stateHash, float blend)
    {
        if (characterAnimator == null || combatLayer < 0) return;
        if (characterAnimator.HasState(combatLayer, stateHash))
            characterAnimator.CrossFadeInFixedTime(stateHash, blend, combatLayer, 0f);
    }

    // ------------------------------------------------------------------------
    // Look
    // ------------------------------------------------------------------------

    private void HoldWeapon()
    {
        if (weaponModel == null || characterAnimator == null || !characterAnimator.isHuman) return;
        Transform hand = characterAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        if (hand == null || !FistGrip.TryMeasure(characterAnimator, hand, out FistGrip fist)) return;

        Transform pivot = new GameObject("EnemyWeaponPivot").transform;
        pivot.SetParent(hand, false);
        pivot.localPosition = fist.centre;
        pivot.localRotation = fist.rotation;

        GameObject weapon = Instantiate(weaponModel, pivot, false);
        new HeldWeaponModel { model = weaponModel, length = weaponLength, gripPoint = weaponGripPoint }.FitToGrip(weapon.transform, pivot);
        // Only for show: the timed strike does the damage.
        foreach (Collider collider in weapon.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
    }

    // Darkens the model's own material copies, so DamageFlash still flashes and restores correctly.
    private void ApplyTint()
    {
        if (tint == Color.white) return;
        foreach (Renderer renderer in characterAnimator.GetComponentsInChildren<Renderer>())
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_Color")) material.color *= tint;
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", material.GetColor("_BaseColor") * tint);
            }
    }

    private void Report(string message)
    {
        if (logActions) Debug.Log(name + ": " + message, this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, circleDistance);
    }
}
