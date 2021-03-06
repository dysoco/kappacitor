﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public GameObject lifebarObject;
    Lifebar lifebar;
    PlayerWeapon gunScript;
    ShieldScript shield;

    public float speed;
    float savedSpeed;
    public int hitpoints = 6;

    public AudioClip minimize;
    public AudioClip maximize;

    public AudioClip hitsound;
    public AudioClip pickup;

    new CameraScript camera;

    Animator animator;
    string previousAnimation;

    float horizontal = 0.0f;
    float vertical = 0.0f;

    PlayerGhost ghost;

    public enum State
    {
        Idle,
        Moving,
        Dashing,
        Shielding
    }

    public enum DashState
    {
        Ready,
        Minimizing,
        Dashing,
        Maximizing,
        Cooldown
    }

    public State state = State.Idle;
    public DashState dashState = DashState.Ready;

    float topLeftMin = 112f;
    float topLeftMax = 157f;

    float topMin = 67f;
    float topMax = 112f;

    float topRightMin = 22f;
    float topRightMax = 67f;

    float rightMin = -67f;
    float rightMax = 22f;

    float botMin = -67f;
    float botMax = -112f;

    float leftMin = -112f;
    float leftMax = 157f;

    public float dashCooldown = 6.0f;
    float dashCooldownTimer = 0.0f;

    bool slowed = false;
    float slowTime;
    float slowTimer;

    GameObject cargador_hud;
    GameObject clock;
    SoundManager soundManager;

    public PlayerGhost ghostPrefab;
    bool dying;

    // Use this for initialization
    void Start()
    {
        lifebar = lifebarObject.GetComponent<Lifebar>();
        gunScript = transform.Find("gun").gameObject.GetComponent<PlayerWeapon>();
        shield = transform.Find("rotation").transform.Find("shield").gameObject.GetComponent<ShieldScript>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraScript>();
        animator = GetComponent<Animator>();
        previousAnimation = "player_idle";
        savedSpeed = speed;
        GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("sound_volume");
        cargador_hud = gameObject.transform.GetChild(2).gameObject;
        clock = gameObject.transform.GetChild(3).gameObject;
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        dying = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (dying)
            return;

        if (!reallyDashing())
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
            calculateAngle();
        }

        Vector3 move = new Vector3(horizontal, vertical, 0);
        transform.position += move * speed * Time.deltaTime;

        if (Input.GetButton("Fire1") && !reallyDashing() && state != State.Shielding)
        {
            bool shot = gunScript.Shoot();
            if (shot)
                camera.StartShaking();
        }

        if (Input.GetMouseButtonDown(1) && !reallyDashing())
        {
            switchState(State.Shielding);
        } else if (Input.GetMouseButtonUp(1) && state == State.Shielding && !shield.isDeleted)
        {
            switchState(State.Idle);
        }

        switch (dashState)
        {
            case DashState.Ready:
                bool isDashKeyDown = Input.GetKeyDown(KeyCode.LeftShift);
                if (isDashKeyDown)
                {
                    switchDashState(DashState.Minimizing);
                }
                break;
            case DashState.Cooldown:
                dashCooldownTimer += Time.deltaTime;
                if (dashCooldownTimer >= dashCooldown)
                {
                    switchDashState(DashState.Ready);
                }
                break;
        }

        if (slowed)
        {
            slowTimer += Time.deltaTime;
            if (slowTimer >= slowTime)
            {
                speed = savedSpeed;
                slowTimer = 0.0f;
                slowed = false;
                ShowClock(false);
            }
        }
    }

    public void switchDashState(DashState state)
    {
        switch (state)
        {
            case DashState.Ready:
                dashCooldownTimer = 0.0f;
                break;
            case DashState.Minimizing:
                speed = 0.0f;
                playSound(minimize);
                switchState(State.Dashing);
                break;
            case DashState.Dashing:
                speed = savedSpeed * 3f;
                break;
            case DashState.Maximizing:
                speed = 0.0f;
                playSound(maximize);
                break;
            case DashState.Cooldown:
                speed = savedSpeed;
                switchState(State.Idle);
                break;
        }

        dashState = state;
    }

    public void switchState(State state)
    {
        this.state = state;

        switch (state)
        {
            case State.Shielding:
                shield.Enable();
                gunScript.GetComponentInParent<SpriteRenderer>().enabled = false;
                break;
            case State.Dashing:
                gunScript.GetComponentInParent<SpriteRenderer>().enabled = false;
                horizontal = Input.GetAxisRaw("Horizontal");
                vertical = Input.GetAxisRaw("Vertical");
                calculateAngle();
                break;
            case State.Idle:
            case State.Moving:
                shield.Disable();
                gunScript.GetComponentInParent<SpriteRenderer>().enabled = true;
                break;
        }
        
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "enemybullet" && !reallyDashing() && !dying)
        {
            Damage(1, 1);
        }

        if (col.gameObject.tag == "wall" && reallyDashing())
        {
            switchDashState(DashState.Cooldown);
        }
    }

    public bool addHitpoints(int toAdd)
    {
        switch (hitpoints)
        {
            case 6:
                return false;
            case 5:
                hitpoints += 1;
                break;
            default:
                hitpoints += toAdd;
                break;
        }

        playSound(pickup);
        lifebar.Change(hitpoints);
        return true;
    }

    public void playSound(AudioClip clip)
    {
        GetComponent<AudioSource>().clip = clip;
        GetComponent<AudioSource>().Play();
    }

    void playAnimation(string anim)
    {
        if (state == State.Dashing)
            animator.Play("player_dash_" + anim);
        else if (state == State.Moving)
            animator.Play("player_" + anim);
        else
            animator.Play("player_idle_" + anim);

        previousAnimation = anim;
    }

    // Devuelve el angulo que hay desde el jugador hasta la posicion del mouse
    float getAngle()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector2 distance = mousePos - Camera.main.WorldToScreenPoint(transform.position);
        return Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
    }

    public void calculateAngle()
    {
        if (dying)
            return;

        float angle = getAngle();

        bool isUp = angle > topMin && angle < topMax;
        bool isDown = angle < botMin && angle > botMax;
        bool isLeft = angle > 157 || angle < -112f;
        bool isRight = angle > rightMin && angle < rightMax;
        bool isTopLeft = angle > topLeftMin && angle < topLeftMax;
        bool isTopRight = angle > topRightMin && angle < topRightMax;

        if ((horizontal != 0f || vertical != 0f) && state == State.Idle)
        {
            state = State.Moving;
        }

        if (horizontal == 0f && vertical == 0f)
        {
            state = State.Idle;
        }

        if (isUp)
        {
            playAnimation("up");
        }
        else if (isDown)
        {
            playAnimation("down");
        }
        else if (isLeft)
        {
            playAnimation("left");
        }
        else if (isRight)
        {
            playAnimation("right");
        }
        else if (isTopLeft)
        {
            playAnimation("upleft");
        }
        else if (isTopRight)
        {
            playAnimation("upright");
        }
    }

    bool reallyDashing()
    {
        return dashState == DashState.Dashing || dashState == DashState.Minimizing ||
            dashState == DashState.Maximizing;
    }

    public void Damage(int hit, int enemy)
    {
        playSound(hitsound);

        if (!slowed)
            GetComponent<TurnRed>().Execute();

        hitpoints -= hit;
        lifebar.Change(hitpoints);

        if (hitpoints <= 0)
        {
            if(soundManager != null)
                soundManager.Dead();

            ghost = (PlayerGhost)Instantiate(ghostPrefab, this.transform.position, Quaternion.identity);
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            foreach (SpriteRenderer spr in gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                spr.enabled = false;
            }
            ghost.Activate(enemy);
            dying = true;
        }
    }

    public void Slow(float time)
    {
        speed = speed * 0.5f;
        slowTime = time;
        slowed = true;
        ShowClock(true);
    }

    public void ShowCargadorHud(bool show)
    {
        if (show)
            cargador_hud.GetComponent<Animator>().Play("cargador_hud", -1, 0f);

        gameObject.transform.GetChild(2).GetComponent<SpriteRenderer>().enabled = show;
    }

    public void ShowClock(bool show)
    {
        if (!show)
        {
            clock.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
        }
        else
        {
            clock.GetComponent<SpriteRenderer>().enabled = true;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.3f, 0.9f);
        }
    }
}
