using UnityEngine;

// A sword-fighting enemy in the Elden Ring style. It chases the player, circles at a distance
// while it picks an attack, then closes in and commits. Its attacks are telegraphed, can be
// blocked, parried or rolled through, and leave it open afterwards.
//  - Parry its strike, or break its poise with enough hits, and it staggers: hit it then for a riposte.
//  - It flinches when hit while it isn't attacking, but keeps swinging through hits mid-attack.
// Uses the same Animator controller as the player (Speed/MoveX/MoveZ for walking, Combat and Death layers).
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CharacterController))]
public class EnemyMelee : MonoBehaviour, IStaggerable, IHitReceiver, IBackstabbable
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
    [Tooltip("Multiplies the damage of every attack.")]
    [Min(0f)] public float damageMultiplier = 1f;
    [Tooltip("Multiplies how long it stands open after each attack. Lower = more relentless.")]
    [Range(0.1f, 2f)] public float recoveryScale = 1f;
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
    [Tooltip("Off = hyper armour: hits don't interrupt it at all, only a poise break does. Bosses turn this off.")]
    public bool flinches = true;
    [Tooltip("Seconds staggered by a poise break. Parries use Stagger Duration instead, so a parry is the bigger opening.")]
    [Min(0.1f)] public float poiseBreakDuration = 1.2f;
    [Tooltip("Seconds after a poise break before its poise can break again, so it can't be stun-locked. 0 = none.")]
    [Min(0f)] public float poiseBreakCooldown = 0f;
    [Tooltip("If the player lands this many hits within Retaliate Window, it lashes out with its first attack, even mid-combo. 0 = never.")]
    [Min(0)] public int retaliateAfterHits = 0;
    [Min(0.1f)] public float retaliateWindow = 2.5f;
    [Tooltip("Seconds it's knocked down after a backstab.")]
    [Min(0.1f)] public float backstabDownTime = 2f;

    [Header("Look")]
    [Tooltip("Sword model to hold in the right hand. Its blade should point along its Y axis.")]
    public GameObject weaponModel;
    [Min(0.05f)] public float weaponLength = 1f;
    [Range(0f, 1f)] public float weaponGripPoint = 0.08f;
    [Tooltip("Multiplies the model's colours. Dark grey gives a shadowy invader look; white leaves it unchanged.")]
    public Color tint = new Color(0.35f, 0.32f, 0.42f);

    [Tooltip("Riposte damage is multiplied by this on top of the player's riposte multiplier. Raise it on bosses so a parry really pays off.")]
    [Min(1f)] public float riposteBonus = 1f;
    [Tooltip("Seconds it's knocked down after a riposte lands.")]
    [Min(0.1f)] public float riposteDownTime = 1.6f;

    [Header("Gap closer")]
    [Tooltip("Seconds between gap-closing attacks, so it doesn't spam them.")]
    [Min(0f)] public float gapCloserCooldown = 6f;

    [Header("Second phase")]
    [Tooltip("Below this share of its health it enters a second phase: faster and more aggressive. 0 = no second phase.")]
    [Range(0f, 1f)] public float secondPhaseBelow = 0f;
    [Tooltip("Movement and attack speed in the second phase.")]
    [Range(1f, 2f)] public float secondPhaseSpeed = 1.25f;

    [Header("Boss")]
    [Tooltip("Stands still and ignores the player until WakeUp() is called. BossEncounter uses this so a boss waits in its arena.")]
    public bool waitToBeWoken;

    public bool logActions = true;

    private enum State { Idle, Chase, Circle, Approach, Attack, Recover, Flinch, Staggered, Dead }
    private State state;
    private float stateEndsAt;
    private EnemyAttack attack;
    private float attackTime;      // Animation seconds into the current attack.
    private float holdEndsAt;
    private bool struck;
    private bool swooshed;   // The swing sound plays just before the hit, not at the wind-up.
    private float circleSide = 1f;
    private float poiseDamage;
    private float lastHitTime = float.NegativeInfinity;
    private float poiseBreakableAt;
    private readonly System.Collections.Generic.Queue<float> recentHits = new System.Collections.Generic.Queue<float>();
    private float verticalSpeed;
    private Vector3 velocity;
    private bool asleep;
    private bool secondPhase;
    private float attackSpeed = 1f;    // Animation speed during attacks; raised in the second phase.
    private float lunge;               // This attack's lunge, longer for a gap closer.
    private float nextGapCloser;

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
    private static readonly int KnockbackState = Animator.StringToHash("Hit_Knockback");

    public bool IsStaggered => state == State.Staggered;
    public bool IsAsleep => asleep;
    public bool IsAttacking => state == State.Attack;
    public bool InSecondPhase => secondPhase;
    public float RiposteBonus => riposteBonus;
    // Fired once when it enters its second phase, e.g. so effects can flare up.
    public event System.Action EnteredSecondPhase;
    // For sounds and effects (see EnemyVoice).
    // WasStaggered also fires for ripostes and backstabs: anything that knocks it off balance.
    public event System.Action Woke, AttackStarted, WasStaggered, Died;
    // Open to a backstab unless it's mid-swing, already staggered or dead.
    public bool CanBeBackstabbed => state != State.Attack && state != State.Staggered && state != State.Dead;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<Health>();
        health.onDeath.AddListener(Die);
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<Animator>();
        if (characterAnimator != null)
        {
            characterAnimator.applyRootMotion = false;
            // Keep animating even if Unity thinks the model is off screen. Some imported models
            // (like the vampire) confuse that check and would otherwise freeze in their bind pose.
            characterAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            combatLayer = characterAnimator.GetLayerIndex("Combat");
            deathLayer = characterAnimator.GetLayerIndex("Death");
            ApplyTint();
        }
        if (combatLayer < 0)
            Debug.LogWarning("EnemyMelee needs the Ranger controller with a Combat layer. Run Tools > Portaland > Prepare Combat Animations.", this);
    }

    private void Start()
    {
        // Read here rather than in Awake so a BossEncounter's Awake can set it first.
        asleep = waitToBeWoken;
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
            CheckSecondPhase();
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
                if (!asleep && distance < aggroRange) SetState(State.Chase);
                break;

            case State.Chase:
                if (distance > giveUpRange) { SetState(State.Idle); break; }
                Face(toPlayer);
                if (TryGapCloser(distance)) break;
                velocity = Steer(toPlayer.normalized) * runSpeed * Speedup;
                if (distance <= circleDistance + 0.5f) StartCircling();
                break;

            case State.Circle:
                Face(toPlayer);
                if (distance > circleDistance + 2.5f) { SetState(State.Chase); break; }
                // Side-step around the player while holding the distance.
                Vector3 side = Vector3.Cross(Vector3.up, toPlayer.normalized) * circleSide;
                float closeIn = Mathf.Clamp(distance - circleDistance, -1f, 1f);
                velocity = (side + toPlayer.normalized * closeIn).normalized * strafeSpeed * Speedup;
                if (Time.time >= stateEndsAt) ChooseAttack();
                break;

            case State.Approach:
                Face(toPlayer);
                if (distance <= attack.reach * 0.85f) StartAttack(attack);
                else if (Time.time >= stateEndsAt) StartCircling();
                else velocity = Steer(toPlayer.normalized) * runSpeed * Speedup;
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

    // Starts fighting straight away, wherever the player is.
    public void WakeUp()
    {
        if (!asleep || state == State.Dead) return;
        asleep = false;
        if (state == State.Idle) SetState(State.Chase);
        Report("Woken");
        Woke?.Invoke();
    }

    private float Speedup => secondPhase ? secondPhaseSpeed : 1f;

    // Lunges across the gap with a gap-closing attack when the player is too far away.
    private bool TryGapCloser(float distance)
    {
        if (attacks == null || Time.time < nextGapCloser) return false;
        foreach (EnemyAttack closer in attacks)
        {
            if (closer.closesFrom <= 0f || distance > closer.closesFrom || distance < closer.reach) continue;
            nextGapCloser = Time.time + gapCloserCooldown;
            StartAttack(closer);
            lunge = Mathf.Max(closer.lunge, distance - closer.reach * 0.6f);
            return true;
        }
        return false;
    }

    private void CheckSecondPhase()
    {
        if (secondPhase || secondPhaseBelow <= 0f || health.maxHealth <= 0) return;
        if (health.CurrentHealth <= 0 || (float)health.CurrentHealth / health.maxHealth > secondPhaseBelow) return;
        secondPhase = true;
        attackSpeed = secondPhaseSpeed;
        poiseDamage = 0f;
        // A short pause to show the change before it comes back faster.
        SetState(State.Recover, 1.2f);
        PlayCombatState(StaggerState, 0.1f);
        Report("Second phase");
        EnteredSecondPhase?.Invoke();
    }

    private void StartCircling()
    {
        circleSide = Random.value < 0.5f ? -1f : 1f;
        SetState(State.Circle, Random.Range(circleTime.x, circleTime.y) / (Speedup * Speedup));
    }

    private void ChooseAttack()
    {
        if (attacks == null || attacks.Length == 0) { StartCircling(); return; }
        EnemyAttack[] normal = System.Array.FindAll(attacks, a => a.closesFrom <= 0f);
        if (normal.Length == 0) normal = attacks;
        attack = normal[Random.Range(0, normal.Length)];
        SetState(State.Approach, 3f);
    }

    // ------------------------------------------------------------------------
    // Attacks
    // ------------------------------------------------------------------------

    private void StartAttack(EnemyAttack next)
    {
        attack = next;
        attackTime = 0f;
        lunge = next.lunge;
        holdEndsAt = float.NegativeInfinity;
        struck = false;
        SetState(State.Attack);
        PlayCombatState(Animator.StringToHash(attack.state), 0.1f);
        Report(attack.label);
        swooshed = false;
        AttackStarted?.Invoke();
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
        if (characterAnimator != null) characterAnimator.speed = holding ? 0.05f : attackSpeed;
        if (!holding) attackTime += Time.deltaTime * attackSpeed;

        if (attackTime < attack.trackUntil || holding) Face(toPlayer);

        // Lunge in during the wind-up, but don't walk through the player.
        if (!struck && !holding && attack.hitTime > 0f && distance > 1.2f)
            velocity = transform.forward * (lunge / attack.hitTime) * attackSpeed;

        if (!swooshed && attackTime >= attack.hitTime - 0.15f)
        {
            swooshed = true;
            CombatAudio.Play(CombatSound.Swing, transform.position + Vector3.up);
        }

        if (!struck && attackTime >= attack.hitTime)
        {
            struck = true;
            Strike();
            if (state != State.Attack) return; // A parry staggered it.
        }

        if (attackTime >= attack.duration)
        {
            // The second phase chains more often and recovers faster.
            float chance = secondPhase ? Mathf.Min(1f, attack.followUpChance + 0.3f) : attack.followUpChance;
            bool combo = attack.followUp >= 0 && attack.followUp < attacks.Length && Random.value < chance;
            if (combo) StartAttack(attacks[attack.followUp]);
            else SetState(State.Recover, attack.recovery * recoveryScale / Speedup);
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
            PlayerDefence.HitResult result = playerDefence.ReceiveHit(attack.damage * damageMultiplier, transform.position);
            Report("Strike: " + result);
            if (result == PlayerDefence.HitResult.Parried) Stagger(true);
        }
        else
        {
            player.TakeDamage(attack.damage * damageMultiplier);
            Report("Strike: Damaged");
        }
    }

    // ------------------------------------------------------------------------
    // Getting hit
    // ------------------------------------------------------------------------

    public void OnHit(int damage)
    {
        WakeUp();
        if (state == State.Dead || state == State.Staggered) return;
        lastHitTime = Time.time;
        if (Time.time >= poiseBreakableAt) poiseDamage += damage;

        if (poiseDamage >= poise)
        {
            poiseDamage = 0f;
            poiseBreakableAt = Time.time + poiseBreakDuration + poiseBreakCooldown;
            Report("Poise broken");
            Stagger(false);
        }
        else if (Retaliate())
        {
            // Punished a greedy combo.
        }
        else if (flinches && state != State.Attack)
        {
            // Outside an attack every hit makes it flinch; mid-attack it swings on through.
            SetState(State.Flinch, flinchTime);
            PlayCombatState(HitState, 0.05f);
        }
    }

    // Too many hits too quickly and it cuts into the player's combo with its first attack.
    private bool Retaliate()
    {
        if (retaliateAfterHits <= 0 || attacks == null || attacks.Length == 0) return false;
        recentHits.Enqueue(Time.time);
        while (recentHits.Count > 0 && Time.time - recentHits.Peek() > retaliateWindow) recentHits.Dequeue();
        if (recentHits.Count < retaliateAfterHits || state == State.Attack) return false;
        recentHits.Clear();
        Report("Retaliates");
        StartAttack(attacks[0]);
        return true;
    }

    // Knocked off balance by a parry or poise break: open to a riposte.
    private void Stagger(bool parried)
    {
        SetState(State.Staggered, parried ? staggerDuration : poiseBreakDuration);
        if (parried && parryPushback > 0f) controller.Move(-transform.forward * parryPushback);
        PlayCombatState(StaggerState, 0.05f);
        Report("Staggered");
        WasStaggered?.Invoke();
    }

    // A riposte landed: knocked down hard (whole body), like a backstab.
    public void EndStagger()
    {
        if (state != State.Staggered) return;
        SetState(State.Recover, riposteDownTime);
        if (characterAnimator != null && deathLayer >= 0 && characterAnimator.HasState(deathLayer, KnockbackState))
            characterAnimator.CrossFadeInFixedTime(KnockbackState, 0.05f, deathLayer, 0f);
        else
            PlayCombatState(HitState, 0.05f);
        Report("Riposted");
        WasStaggered?.Invoke();
    }

    // Stabbed in the back: knocked stumbling (whole body) and slow to recover.
    public void OnBackstabbed()
    {
        WakeUp();
        SetState(State.Recover, backstabDownTime);
        if (characterAnimator != null && deathLayer >= 0 && characterAnimator.HasState(deathLayer, KnockbackState))
            characterAnimator.CrossFadeInFixedTime(KnockbackState, 0.05f, deathLayer, 0f);
        else
            PlayCombatState(HitState, 0.05f);
        Report("Backstabbed");
        WasStaggered?.Invoke();
    }

    private void Die()
    {
        SetState(State.Dead);
        // The body shouldn't block the player or catch their attacks.
        controller.enabled = false;
        if (characterAnimator != null && deathLayer >= 0 && characterAnimator.HasState(deathLayer, DeathState))
            characterAnimator.CrossFadeInFixedTime(DeathState, 0.1f, deathLayer, 0f);
        Report("Died");
        Died?.Invoke();
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
