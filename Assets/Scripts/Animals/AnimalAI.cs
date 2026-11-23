using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wanders round its home spot, legs it when the player gets close or hits it
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

    // anim speeds, bump up if the feet slide
    public float slowRunAnimSpeed = 0.5f;   // run anim when its faking a walk
    public float walkAnimSpeed = 1f;        // for animals with a real walk
    public float runAnimSpeed = 1f;         // when its running away

    public float walkSpeed = 1f;
    public float runSpeed = 4f;
    public float turnSpeed = 360f;
    public float wanderRadius = 8f;                  // how far it strays from spawn
    public Vector2 idleTime = new Vector2(2f, 5f);   // chill time between walks
    public float fleeRange = 6f;            // player closer than this = run
    public float safeRange = 14f;           // stops running past this
    public float gravity = 20f;
    public float corpseTime = 10f;          // body stays this long

    enum State { Idle, Wander, Flee, Dead }
    State state;
    float stateEndsAt;
    Vector3 home;
    Vector3 target;
    float fallSpeed;
    string playing;
    CharacterController controller;
    Health health;

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
        home = transform.position;
        GoIdle();
    }

    void Update()
    {
        if (state == State.Dead) return;

        bool playerOk = player != null && !player.IsDead();
        float toPlayer = playerOk ? Vector3.Distance(transform.position, player.transform.position) : Mathf.Infinity;

        if (state != State.Flee && toPlayer < fleeRange) state = State.Flee;

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
        }

        // stick to the ground
        if (controller.isGrounded) fallSpeed = -1f;
        else fallSpeed -= gravity * Time.deltaTime;
        move.y = fallSpeed;
        controller.Move(move * Time.deltaTime);
    }

    // player hit it, run for it
    public void OnHit(int damage)
    {
        if (state == State.Dead || player == null) return;
        state = State.Flee;
        stateEndsAt = Time.time + 3f;   // keeps running a bit even if you hit it from far
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
        Play(deathState);
        controller.enabled = false;   // cant bump into the body
        Destroy(gameObject, corpseTime);
    }

    static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    // yellow = wander area, red = flee range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? home : transform.position, wanderRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRange);
    }
}