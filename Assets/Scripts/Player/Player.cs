using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public enum AgeState    {   Baby, MidAge, Ded   }

public class Player : MonoBehaviour
{
    [Header("Player_Movement")]
    public Rigidbody2D Player_body;
    public float maxSpeed => CurrentAge switch
    {
        AgeState.Baby => 5.0f,
        AgeState.MidAge => 5.0f,
        AgeState.Ded => 3f,
        _ => 3.0f
    };
    public float jumpForce => CurrentAge switch
    {
        AgeState.Baby => 6.8f,
        AgeState.MidAge => 8f,
        AgeState.Ded => 3f,
        _ => 2.0f
    };
    private int _jumpLimit => CurrentAge switch
    {
        AgeState.Baby => 1, 
        _ => 0 
    };
    public float smoothing => CurrentAge switch
    {
        AgeState.Baby => 2.0f,
        AgeState.MidAge => 10.0f,
        AgeState.Ded => 4f,
        _ => 2.0f
    };

    private float smoothedInput;
    float targetInput = 0f;

    [Header("Sprite_Render")]
    private SpriteRenderer Player_model;
    public Sprite babySprite;
    public Sprite midAgeSprite;
    public Sprite dedSprite;
    

    [Header("Box")]
    private float Box_radius = 1f;
    public LayerMask Box_Layer;
    private bool Is_near_to_Box;
    public Transform Box_Check;
    public GameObject BB;

    [Header("Ground")]
    private float Ground_radius = 0.2f;
    public LayerMask Ground_Layer;
    private bool Is_Grounded;
    public Transform Ground_Check;

    [Header("Player_characteristics")]
    private Animator animator;
    public int RemainingJumps;

    private bool isDead;
    public bool isFlip; 
    
    private bool isUmbrella;

    [Header("Player_additional")]
    [SerializeField] private ParticleSystem walking_particles;
    [SerializeField] private ParticleSystem Death_particles;
    private ParticleSystem Death_particles_Instance;



    [Header("SFX")]
    private AudioSource audio_source;
    public AudioClip Jump_Clip;
    public AudioClip Watch_Clip;
    public AudioClip Death_Clip_Young;
    public AudioClip Death_Clip_Old;

    [Header("Lever")]
    public float Lever_radius = 2f;
    public bool Is_near_to_Lever = false;
    public LayerMask Lever_Layer;
    public Transform Lever_Check;
    public GameObject LL;
    public Transform[] children;

    [SerializeField] private AgeState ageState;
    public AgeState CurrentAge
    {
        get => ageState;
        set
        {
            if (ageState == value) return;
            ageState = value;
            UpdateColliderParameters();
            UpdatePlayerVisual();
        }
    }

    [Header("Colliders")]
    public CapsuleCollider2D playerCollider => cachedCollider;
    private CapsuleCollider2D cachedCollider;
    private Vector2 babyOffset;
    private Vector2 babySize;

    [Header ("UI_Elements")]
    [SerializeField] private Image F_Image;
    [SerializeField] private float Current_Alpha_Value = 1;

    public GameObject PP;

    //                                              Unity functions


    private void Awake() { Resume(); this.enabled = true; }

    void Start()
    {
        audio_source = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        F_Image = GameObject.FindWithTag("Fading_Screen").GetComponent<Image>();
        Player_body = GetComponent<Rigidbody2D>();
        Player_model = GetComponent<SpriteRenderer>();
        F_Image.color = new Color(0, 0, 0, 1);
        BB = GameObject.FindWithTag("Box");
        LL = GameObject.FindWithTag("Lever");
        PP = GameObject.FindWithTag("Particles_Walk");


        cachedCollider = GetComponent<CapsuleCollider2D>();
        if (cachedCollider != null)
        {
            babySize = cachedCollider.size;
            babyOffset = cachedCollider.offset;
    
            UpdateColliderParameters();
        }
    }
    void Update()
    {
        PlayerInput.GatherInput();

        // Непроверенное
        // Fliping the sprite
        if (targetInput < 0f)
        {
            Player_model.flipX = true;
        }
        else
        {
            Player_model.flipX = false;
        }
        if ((CurrentAge == AgeState.Ded)  ) // Umbrella_falling_and_Lever_activating
        {
            if ((Is_Grounded == true))
            {
                Player_body.gravityScale = 1f;
                isUmbrella = false;
            }
            Player_body.mass = 1f;
            if (PlayerInput.RKeyPressed && isUmbrella == false && Is_Grounded == false)
            {
                Player_body.gravityScale = 0.1f;
                Player_body.linearVelocity = new Vector2(Player_body.linearVelocity.x, 0.3f);
                isUmbrella = true;
            }
            else if ((PlayerInput.RKeyPressed && isUmbrella == true) && (Is_Grounded == false))
            {
                Player_body.gravityScale = 1f;
                isUmbrella = false;
            }
            if (Is_near_to_Lever == true && PlayerInput.EKeyPressed)
            {
                
                if (children[2] != null)
                {
                    Destroy(LL.GetComponent<Transform>().GetChild(1).gameObject);
                }
            }
            if (Is_near_to_Lever == false)
            {
                Player_model.flipX = false;
            }
            
        }
        if ((CurrentAge == AgeState.MidAge) || (CurrentAge == AgeState.Ded))
        {
            RemainingJumps = 0;
            
        }
        if ((CurrentAge == AgeState.Baby))
        {
            Player_body.mass = 1f;
        }
        if (Is_Grounded)
        {
            if (PP != null) { PP.SetActive(true); }
            RemainingJumps = _jumpLimit;
            if (PlayerInput.JumpPressed)
            {
                Player_body.linearVelocity = new Vector2(Player_body.linearVelocity.x, jumpForce);
                PlaySFX(Jump_Clip);
            }
        }
        else 
        {
            if (PP != null) { PP.SetActive(false); }
        }
        if ((RemainingJumps != 0) && (Is_Grounded == false))
        {
            if (PlayerInput.JumpPressed)
            {
                Player_body.linearVelocity = new Vector2(Player_body.linearVelocity.x, jumpForce);
                PlaySFX(Jump_Clip);
                RemainingJumps -= 1;
            }
        }
        if (Is_near_to_Box  && CurrentAge == AgeState.MidAge) 
        {
            Player_body.mass = 1000f;
            if ((Box_Check.transform.position.y > BB.transform.position.y + 1))
            {
                return;
            }
            if (PlayerInput.EKeyHeld)
            {
                if (Box_Check.transform.position.x < BB.transform.position.x)
                {
                    if (smoothedInput < 0)
                    {
                        BB.transform.position = new Vector2(Box_Check.transform.position.x + 1.5f, Box_Check.transform.position.y);
                    }
                }
                else if (Box_Check.transform.position.x > BB.transform.position.x)
                {
                    if (smoothedInput > 0)
                    {
                        BB.transform.position = new Vector2(Box_Check.transform.position.x - 1.5f, Box_Check.transform.position.y);
                    }
                }
            } 
        }
    }

    void FixedUpdate()
    {
        // Moving
        targetInput = 0f;
        if (PlayerInput.DKeyPressed) targetInput = 1f;
        if (PlayerInput.AKeyPressed) targetInput = -1f;

        SetAnimation(targetInput);
        smoothedInput = Mathf.MoveTowards(smoothedInput, targetInput, smoothing * Time.deltaTime);
        Player_body.linearVelocity = new Vector2(maxSpeed * smoothedInput, Player_body.linearVelocity.y);
        Is_Grounded = Physics2D.OverlapCircle(Ground_Check.position, Ground_radius, Ground_Layer);

        // Непроверенное
        Is_near_to_Lever = Physics2D.OverlapCircle(Lever_Check.position, Lever_radius, Lever_Layer);
        Is_near_to_Box = Physics2D.OverlapCircle(Box_Check.position, Box_radius, Box_Layer);

        if (F_Image.color.a != 0 && isDead == false)
        {
            Current_Alpha_Value -=  Time.deltaTime;
            F_Image.color = new Color(0, 0, 0, Current_Alpha_Value);
        }
        else if (isDead == true )
        {
            Current_Alpha_Value += Time.deltaTime *10f;
            F_Image.color = new Color(0, 0, 0, Current_Alpha_Value);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) // changes current "Main" box
    {
        if (other.CompareTag("Box") && other.transform.parent != null)
        {
            BB = other.transform.parent.gameObject;
        }
        if (other.gameObject.tag == "Lever" && other.transform.parent != null)
        {
            LL = other.transform.parent.gameObject;
            children = LL.GetComponentsInChildren<Transform>();
        }
        if (other.CompareTag("Watch"))
        {
            PlaySFX(Watch_Clip);
        }
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage_Pike"))
        {
            Kill("Was Pierced by Thorns");
        }
    }


    //                                              Custom functions


    private void SetAnimation(float targetInput)
    {
        string age = CurrentAge switch
        {
            AgeState.Baby => "Kid",
            AgeState.Ded => "Ded",
            _ => "Parent"
        };

        string animName = (Is_Grounded, targetInput == 0, Player_body.linearVelocityY > 0, isUmbrella, PlayerInput.EKeyHeld) switch
        {
            (false, _, false, false, false)  => $"{age}_Fall_Animation",
            (false, _, false, true, false)   => $"{age}_Umbrella_Animation",
            (true, true, _, false, false )   => $"{age}_Idle0_Animation",
            (false, _, true, false, false)   => $"{age}_Jump_Animation",
            (true, false, _, false, false)   => $"{age}_Run_Animation",
            (true, _, false, false, true)    => $"{age}_Int_Animation",
            _                                => $"{age}_Idle0_Animation",
        };

        animator.Play(animName);
    }

    private void UpdateColliderParameters() 
    {
        if (cachedCollider == null) return;

        switch (ageState)
        {
            case AgeState.Baby:
                cachedCollider.size = babySize;
                cachedCollider.offset = babyOffset;
                break;

            case AgeState.MidAge:
                float midHeight = babySize.y * 1.2f;
                cachedCollider.size = new Vector2(babySize.x, midHeight);
                cachedCollider.offset = new Vector2(babyOffset.x, babyOffset.y -0.30f );
                break;

            case AgeState.Ded:
                float dedHeight = babySize.y * 1.2f;
                cachedCollider.size = new Vector2(babySize.x, dedHeight);
                cachedCollider.offset = new Vector2(babyOffset.x, babyOffset.y - (dedHeight - babySize.y) / 2f);
                break;
        }
    }
    private void UpdatePlayerVisual()
    {

        Player_model.sprite = ageState switch
        {
            AgeState.Baby => babySprite,
            AgeState.MidAge => midAgeSprite,
            AgeState.Ded => dedSprite,
            _ => Player_model.sprite
        };
    }

    public void DeathSound()
    {
        if ((CurrentAge == AgeState.MidAge) || (CurrentAge == AgeState.Ded))
        {
            PlaySFX(Death_Clip_Old);
        }
        else
        {
            PlaySFX(Death_Clip_Young);
        }
    }

    public void Kill(string cause = "Curiosity")
    {
        if (isDead) return;

        Debug.Log($"Entity was killed by: {cause}");
        Die();
    }

    private void Die()
    {
        isDead = true;
        DeathSound();
        Death_particles_Instance = Instantiate(Death_particles, Ground_Check.transform.position, Quaternion.identity);
        
        Invoke("LoadSceneDelay", 1f);

        Player_model.enabled = false;
        cachedCollider.enabled = false;
        if (TryGetComponent<Rigidbody2D>(out var rb)) rb.simulated = false;
    }
    // <<< Переделать корутинами
    private void LoadSceneDelay()
    {
        LoadScene("");
    }
    
    public void LoadScene(string sceneName = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Error: Scene '{sceneName}' isn't found! Add it to Build Settings..");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
    // >>>
    public void PlaySFX(AudioClip audioClip)
    {
        audio_source.clip = audioClip;
        audio_source.Play();
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        // Coming Soon: turn off sounds
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        // Coming Soon: turn on sounds
    }
}