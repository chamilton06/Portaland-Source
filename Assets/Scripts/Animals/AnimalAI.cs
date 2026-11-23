using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wanders round its home spot. passive ones leg it from the player,
// aggressive ones (wolf, bear) chase you down and bite
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
public class AnimalAI : MonoBehaviour, IHitReceiver
{
    public PlayerStats player;              // finds it itself if empty
    public Animator anim;                   // the models animator, finds it if empty

    // state names from the animator, copy them exactly
    public string idleState = "Idle";
    public string walkState = "";           // empty = plays run slowed down
    public string runState = "Run";
    public string deathState = "Dead";
    public string hitState = "";            // flinch when parried, optional

    // anim speeds, bump up if the feet slide
    public float slowRunAnimSpeed = 0.5f;   // run anim when its faking a walk
    public float walkAnimSpeed = 1f;        // for animals with a real walk
    public float runAnimSpeed = 1f;         // running away or chasing

    public float walkSpeed = 1f;
    public float runSpeed = 4f;
    public float turnSpeed = 360f;
    public float wanderRadius = 8f;                  // how far it strays from spawn
    public Vector2 idleTime = new Vector2(2f, 5f);   // chill time between walks
    public float fleeRange = 6f;            // passive: player closer than this = run
    public float safeRange = 14f;           // passive: stops running past this
    public float gravity = 20f;
    public float corpseTime = 10f;          // body stays this long

    // only used when aggressive is on
    public bool aggressive = false;
    public string[] attackStates;           // picks one at random each time
    public float attackDamage = 15f;
    public float damageMultiplier = 1f;     // night time cranks this up later
    public float chaseRange = 12f;          // notices you this close
    public float giveUpRange = 25f;         // stops chasing past this
    public float attackRange = 1.8f;        // starts the bite this close
    public float attackHitTime = 0.4f;      // secs into the attack when it lands
    public float attackDuration = 1.2f;     // whole attack anim length
    public float attackCooldown = 0.8f;     // breather between attacks
    public float parryStun = 1.5f;          // stunned this long when you parry it

    enum State { Idle, Wander, Flee, Chase, Attack, Dead }
    State state;
    float stateEndsAt;
    float attackStartedAt;
    float nextAttackAt;
    bool bitThisAttack;
    Vector3 home;
    Vector3 target;
    float fallSpeed;
    string playing;
    CharacterController controller;
    Health health;
    PlayerDefence defence;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<Health>();
        health.onDeath.AddListener(Die);
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.applyRootMotion = false;
    }

    void Start()
    {
        if (player == null) player = FindFirstObjectByType<PlayerStats>();
        if (player != null) defence = player.GetComponent<PlayerDefence>();
        home = transform.position;
        GoIdle();
    }

    void Update()
    {
        if (state == State.Dead) return;

        bool playerOk = player != null && !player.IsDead();
        float toPlayer = playerOk ? Vector3.Distance(transform.position, player.transform.position) : Mathf.Infinity;

        // spotted the player
        if (state == State.Idle || state == State.Wander)
        {
            if (aggressive && toPlayer < chaseRange) state = State.Chase;
            else if (!aggressive && toPlayer < fleeRange) state = State.Flee;
        }

        Vector3 move = Vector3.zero;
        switch (state)
        {
            case State.Idle:
                Play(idleState);
                if (Time.time > stateEndsAt) PickWanderSpot();
                break;

            case State.Wander:
                move = Steer(target, walkSpeed);
                PlayWalk();
                if (Flat(target - transform.position).magnitude < 0.5f || Time.time > stateEndsAt) GoIdle();
                break;

            case State.Flee:
                if (!playerOk) { GoIdle(); break; }
                Vector3 away = transform.position + Flat(transform.position - player.transform.position).normalized * 5f;
                move = Steer(away, runSpeed);
                Play(runState, runAnimSpeed);
                if (toPlayer > safeRange && Time.time > stateEndsAt) GoIdle();
                break;

            case State.Chase:
                if (!playerOk || toPlayer > giveUpRange) { GoIdle(); break; }
                if (toPlayer > attackRange)
                {
                    move = Steer(player.transform.position, runSpeed);
                    Play(runState, runAnimSpeed);
                }
                else
                {
                    Steer(player.transform.position, 0f);   // just face you
                    if (Time.time >= nextAttackAt) StartAttack();
                    else Play(idleState);                   // waiting on cooldown
                }
                break;

            case State.Attack:
                if (playerOk) Steer(player.transform.position, 0f);
                float t = Time.time - attackStartedAt;
                if (!bitThisAttack && t >= attackHitTime)
                {
                    bitThisAttack = true;
                    TryBite();
                }
                if (state == State.Attack && t >= attackDuration)
                {
                    nextAttackAt = Time.time + attackCooldown;
                    state = State.Chase;
                }
                break;
        }

        // stick to the ground
        if (controller.isGrounded) fallSpeed = -1f;
        else fallSpeed -= gravity * Time.deltaTime;
        move.y = fallSpeed;
        controller.Move(move * Time.deltaTime);
    }

    void StartAttack()
    {
        if (attackStates == null || attackStates.Length == 0) return;
        state = State.Attack;
        attackStartedAt = Time.time;
        bitThisAttack = false;
        playing = null;   // so the same attack can play twice in a row
        Play(attackStates[Random.Range(0, attackStates.Length)]);
    }

    // lands if youre still close and in front of it
    void TryBite()
    {
        if (player == null || player.IsDead()) return;
        Vector3 toPlayer = Flat(player.transform.position - transform.position);
        if (toPlayer.magnitude > attackRange + 0.5f) return;            // you backed off
        if (Vector3.Angle(transform.forward, toPlayer) > 70f) return;   // you dodged round it

        float damage = attackDamage * damageMultiplier;
        if (defence != null && defence.isActiveAndEnabled)
        {
            PlayerDefence.HitResult result = defence.ReceiveHit(damage, transform.position);
            if (result == PlayerDefence.HitResult.Parried) GetParried();
        }
        else player.TakeDamage(damage);
    }

    // nice parry, it reels back
    void GetParried()
    {
        state = State.Chase;
        nextAttackAt = Time.time + parryStun;
        if (!string.IsNullOrEmpty(hitState))
        {
            playing = null;
            Play(hitState);
        }
    }

    // player hit it
    public void OnHit(int damage)
    {
        if (state == State.Dead || player == null) return;
        if (aggressive)
        {
            if (state != State.Attack) state = State.Chase;   // oh you wanna fight
        }
        else
        {
            state = State.Flee;
            stateEndsAt = Time.time + 3f;   // keeps running a bit even if you hit it from far
        }
    }

    void GoIdle()
    {
        state = State.Idle;
        stateEndsAt = Time.time + Random.Range(idleTime.x, idleTime.y);
    }

    void PickWanderSpot()
    {
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        target = home + new Vector3(r.x, 0f, r.y);
        state = State.Wander;
        stateEndsAt = Time.time + 8f;   // gives up if it gets stuck on something
    }

    // turns towards the spot and walks where its facing so it curves nicely
    Vector3 Steer(Vector3 to, float speed)
    {
        Vector3 dir = Flat(to - transform.position);
        if (dir.sqrMagnitude < 0.01f) return Vector3.zero;
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
        return transform.forward * speed;
    }

    void PlayWalk()
    {
        if (!string.IsNullOrEmpty(walkState)) Play(walkState, walkAnimSpeed);
        else Play(runState, slowRunAnimSpeed);   // rabbit + squirrel got no walk
    }

    void Play(string stateName, float speed = 1f)
    {
        if (anim == null || string.IsNullOrEmpty(stateName)) return;
        anim.speed = speed;
        if (playing == stateName) return;
        playing = stateName;
        anim.CrossFade(stateName, 0.15f);
    }

    void Die()
    {
        state = State.Dead;
        playing = null;
        Play(deathState);
        controller.enabled = false;   // cant bump into the body
        Destroy(gameObject, corpseTime);
    }

    static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    // yellow = wander area, red = flee or chase range, orange = bite range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? home : transform.position, wanderRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggressive ? chaseRange : fleeRange);
        if (aggressive)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}