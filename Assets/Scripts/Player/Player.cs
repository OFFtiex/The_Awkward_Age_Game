using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum AgeState    {   Youth, Prime, Aging   }

public class Player : MonoBehaviour
{
    private Rigidbody2D Player_body;

    [Header("Player Movement")]
    public float Smoothing { get; private set; }
    public float MaxSpeed { get; private set; }
    public int JumpLimit { get; private set; }

    private float _smoothedInput;
    
    float targetInput;

    [Header("Jump Settings")]
    public float JumpForce;          
    public float JumpCancelForce = 0.3f;
    public float MinJumpVelocity = 2f;

    private float _jumpTimeCounter;
    private int  _remainingJumps;
    private bool _isJumpIntent;
    private bool _isJumping;

    [Header("Feature sets")]
    private AgeStats _youthStats;
    private AgeStats _primeStats;
    private AgeStats _agingStats;

    [Header("Sprite Render")]
    [SerializeField] private Sprite _youthSprite;
    [SerializeField] private Sprite _primeSprite;
    [SerializeField] private Sprite _agingSprite;
    private SpriteRenderer PlayerModel;
    private Animator animator;

    [Header("Box")]//FIXME
    private readonly float Box_radius = 1f;
    [SerializeField] private LayerMask Box_Layer;
    private bool Is_near_to_Box;
    [SerializeField] private Transform Box_Check;
    [SerializeField] private GameObject BB;

    [Header("Ground")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    private readonly float _groundRadius = 0.2f;
    private bool _isGrounded;

    [Header("Player additional")]
    [SerializeField] private ParticleSystem walking_particles;
    [SerializeField] private ParticleSystem Death_particles;
    [SerializeField] private ParticleSystem Death_particles_Instance;
    public GameObject PP;
    private AgeState _currentAge;
    private AgeStats _selectedStats => _currentAge switch
    {
        AgeState.Prime => _primeStats,
        AgeState.Aging => _agingStats,
        _ => _youthStats
    };
    public AgeState CurrentAge
    {
        get => _currentAge;
        set
        {
            if (_currentAge == value) return;
            _currentAge = value;

            UpdatePlayerCollider();
            UpdatePlayerVisual();
            UpdatePlayerCharacteristics();
        }
    }

    private bool _isUmbrella;
    private bool _isDead;
    


    [Header("SFX")]
    [SerializeField] private AudioClip Death_Clip_Young;
    [SerializeField] private AudioClip Death_Clip_Old;
    [SerializeField] private AudioClip Jump_Clip;
    
    [SerializeField] private AudioSource audio_source;

    // FIXME
    [Header("Lever")]
    public float Lever_radius = 2f;
    public bool Is_near_to_Lever = false;
    public LayerMask Lever_Layer;
    public Transform Lever_Check;
    public GameObject LL;
    public Transform[] children;

    [Header("Colliders")]
    private CapsuleCollider2D _cachedCollider;

    [Header ("UI Elements")]
    [SerializeField] private Image F_Image;
    [SerializeField] private float Current_Alpha_Value = 1;


    //                                              Unity functions


    private void Awake() 
    {
        Resume(); this.enabled = true;
        Player_body = GetComponent<Rigidbody2D>();
        _cachedCollider = GetComponent<CapsuleCollider2D>();
        PlayerModel = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audio_source = GetComponent<AudioSource>();

        _youthStats = new AgeStats(5f, 6.615f, 2, 50f, _youthSprite, new Vector2(0.78f, 0.95f), new Vector2(-0.11f, -0.02f));
        _primeStats = new AgeStats(5f, 8f, 1, 10f, _primeSprite, new Vector2(0.9f, 1.8f), new Vector2(-0.05f, -0.08f));
        _agingStats = new AgeStats(3f, 3f, 1, 3f, _agingSprite, new Vector2(0.9f, 1.5f), new Vector2(0f, 0f));

        _currentAge = AgeState.Youth;

        UpdatePlayerCollider();
        UpdatePlayerVisual();
        UpdatePlayerCharacteristics();
    }

    void Start()
    {
        F_Image = GameObject.FindWithTag("Fading_Screen").GetComponent<Image>();
        F_Image.color = new Color(0, 0, 0, 1);

        // FIXME
        BB = GameObject.FindWithTag("Box");
        LL = GameObject.FindWithTag("Lever");
        PP = GameObject.FindWithTag("Particles_Walk");

    }
    void Update()
    {
        PlayerInput.GatherInput();

        // Jump
        if (PlayerInput.SpacePressed && _remainingJumps > 0) _isJumpIntent = true;

        // FIXME: Spaghetti code

        // Fliping the sprite
        if (targetInput < 0f)
        {
            PlayerModel.flipX = true;
        }
        else
        {
            PlayerModel.flipX = false;
        }
        if ((CurrentAge == AgeState.Aging)  ) // Umbrella_falling_and_Lever_activating
        {
            if ((_isGrounded == true))
            {
                Player_body.gravityScale = 1f;
                _isUmbrella = false;
            }
            Player_body.mass = 1f;
            if (PlayerInput.RKeyPressed && _isUmbrella == false && _isGrounded == false)
            {
                Player_body.gravityScale = 0.1f;
                Player_body.linearVelocity = new Vector2(Player_body.linearVelocity.x, 0.3f);
                _isUmbrella = true;
            }
            else if ((PlayerInput.RKeyPressed && _isUmbrella == true) && (_isGrounded == false))
            {
                Player_body.gravityScale = 1f;
                _isUmbrella = false;
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
                PlayerModel.flipX = false;
            }
        }
        if ((CurrentAge == AgeState.Prime) || (CurrentAge == AgeState.Aging))
        {
            _remainingJumps = 0;
        }
        if ((CurrentAge == AgeState.Youth))
        {
            Player_body.mass = 1f;
        }
        
        if (Is_near_to_Box  && CurrentAge == AgeState.Prime) 
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
                    if (_smoothedInput < 0)
                    {
                        BB.transform.position = new Vector2(Box_Check.transform.position.x + 1.5f, Box_Check.transform.position.y);
                    }
                }
                else if (Box_Check.transform.position.x > BB.transform.position.x)
                {
                    if (_smoothedInput > 0)
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
        if (PlayerInput.DKeyHeld) targetInput = 1f;
        if (PlayerInput.AKeyHeld) targetInput = -1f;

        SetAnimation(targetInput);
        _smoothedInput = Mathf.MoveTowards(_smoothedInput, targetInput, Smoothing * Time.deltaTime);
        Player_body.linearVelocity = new Vector2(MaxSpeed * _smoothedInput, Player_body.linearVelocity.y);
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundRadius, _groundLayer);

        // Jump
        if (_isJumpIntent || _isJumping) HandleJumpPhysics();

        // FIXME
        Is_near_to_Lever = Physics2D.OverlapCircle(Lever_Check.position, Lever_radius, Lever_Layer);
        Is_near_to_Box = Physics2D.OverlapCircle(Box_Check.position, Box_radius, Box_Layer);

        if (F_Image.color.a != 0 && _isDead == false)
        {
            Current_Alpha_Value -=  Time.deltaTime;
            F_Image.color = new Color(0, 0, 0, Current_Alpha_Value);
        }
        else if (_isDead == true )
        {
            Current_Alpha_Value += Time.deltaTime *10f;
            F_Image.color = new Color(0, 0, 0, Current_Alpha_Value);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) // changes current "Main" box
    {
        // FIXME
        if (other.CompareTag("Box") && other.transform.parent != null)
        {
            BB = other.transform.parent.gameObject;
        }
        if (other.gameObject.tag == "Lever" && other.transform.parent != null)
        {
            LL = other.transform.parent.gameObject;
            children = LL.GetComponentsInChildren<Transform>();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {   
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground_Layer"))
        {
            _remainingJumps = JumpLimit;
            //if (PP != null) { PP.SetActive(true); }
        }
        if (collision.gameObject.CompareTag("Damage_Pike"))
        {
            Kill("Was Pierced by Thorns");
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground_Layer"))
        {
            //if (PP != null) { PP.SetActive(false); }
        }
    }


    //                                              Custom functions


    private void SetAnimation(float targetInput)
    {
        string age = CurrentAge switch
        {
            AgeState.Youth => "Kid",
            AgeState.Aging => "Ded",
            _ => "Parent"
        };

        string animName = (_isGrounded, targetInput == 0, Player_body.linearVelocityY > 0, _isUmbrella, PlayerInput.EKeyHeld) switch
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

    private void UpdatePlayerCollider()
    {
        _cachedCollider.size = _selectedStats.ColliderSize;
        _cachedCollider.offset = _selectedStats.ColliderOffset;
    }
    private void UpdatePlayerVisual()
    {
        PlayerModel.sprite = _selectedStats.VisualSprite;
    }
    private void UpdatePlayerCharacteristics() 
    {
        MaxSpeed  = _selectedStats.MaxSpeed;
        JumpForce = _selectedStats.JumpForce;
        JumpLimit = _selectedStats.JumpLimit;
        Smoothing = _selectedStats.Smoothing;
    }
    private void HandleJumpPhysics()
    {
        if (_isJumpIntent)
        {
            Player_body.linearVelocity = new Vector2(Player_body.linearVelocity.x, JumpForce);
            PlaySFX(Jump_Clip);
            _remainingJumps--;
            _isJumping = true;
            _isJumpIntent = false;
            return;
        }

        if (PlayerInput.SpaceReleased && Player_body.linearVelocity.y > MinJumpVelocity)
        {
            Player_body.linearVelocity = new Vector2(Player_body.linearVelocity.x, Player_body.linearVelocity.y * JumpCancelForce);
        }

        if (Player_body.linearVelocity.y <= 0)
        {
            _isJumping = false;
        }
    }

    public void DeathSound()
    {
        if ((CurrentAge == AgeState.Prime) || (CurrentAge == AgeState.Aging))
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
        if (_isDead) return;

        Debug.Log($"Entity was killed by: {cause}");
        Die();
    }

    private void Die()
    {
        _isDead = true;
        DeathSound();
        Death_particles_Instance = Instantiate(Death_particles, _groundCheck.transform.position, Quaternion.identity);
        
        Invoke("LoadSceneDelay", 1f);

        PlayerModel.enabled = false;
        _cachedCollider.enabled = false;
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