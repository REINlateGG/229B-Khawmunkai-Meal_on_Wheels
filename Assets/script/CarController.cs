using UnityEngine;
using UnityEngine.SceneManagement;

public class CarController : MonoBehaviour
{
    [Header("Movement Setting")]
    public float accelerationForce = 500f;
    public float maxSpeed = 20f;
    public float turnSpeed = 100f;
    public float brakeForce = 1000f;
    public float boostMultiplier = 1.5f;

    [Header("Health Setting")]
    public int maxHP = 10;
    public int currentHP;
    public HealthBar healthBar;

    [Header("Audio Setting")]
    public AudioSource engineSource;
    public AudioClip engineIdleClip;
    public AudioClip engineAccelerateClip;
    public AudioClip brakeClip;
    public AudioClip damageSound;

    [Header("UI")]
    public GameObject gameOverPanel;
    public float restartDelay = 2f;

    [Header("Time Setting")]
    public float timeLimit = 120f;
    private float currentTime;
    public TMPro.TextMeshProUGUI timerText;
    public GameObject timeOverPanel;

    private Rigidbody rb;
    private bool isBraking = false;
    private bool isMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHP = maxHP;
        healthBar.SetMaxHealth(maxHP);
        gameOverPanel.SetActive(false);
        timeOverPanel.SetActive(false);
        currentTime = timeLimit;
        UpdateTimerUI();

        if (engineSource != null && engineIdleClip != null)
        {
            engineSource.clip = engineIdleClip;
            engineSource.loop = true;
            engineSource.Play();
        }
    }

    void FixedUpdate()
    {
        if (currentHP <= 0) return;
        ApplyAcceleration();
        ApplySteering();
        ApplyBrakes();
        UpdateEngineSound();
        UpdateTimer();
    }

    void ApplyAcceleration() // Boost
    {
        float moveInput = Input.GetAxis("Vertical");
        bool isBoosting = Input.GetKey(KeyCode.LeftShift);

        float currentAcceleration = isBoosting ? accelerationForce * boostMultiplier : accelerationForce;

        if (moveInput != 0)
        {
            Vector3 force = transform.forward * moveInput * currentAcceleration * Time.fixedDeltaTime;
            rb.AddForce(force, ForceMode.Acceleration);
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed * (isBoosting ? boostMultiplier : 1));
    }

    void ApplySteering() // เลี้ยว
    {
        float turnInput = Input.GetAxis("Horizontal");
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
            transform.Rotate(0, turn, 0);
        }
    }

    void ApplyBrakes() // Brake
{
    if (Input.GetKey(KeyCode.Space))
    {
        Vector3 brakeForceVector = -rb.linearVelocity.normalized * brakeForce * Time.fixedDeltaTime;
        rb.AddForce(brakeForceVector, ForceMode.Acceleration);

        if (rb.linearVelocity.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (!isBraking)
        {
            PlayBrakeSound();
            isBraking = true;
        }
    }
    else
    {
        isBraking = false;
    }
}


    void UpdateEngineSound()
    {
        if (engineSource == null) return;

        if (isMoving)
        {
            if (engineSource.clip != engineAccelerateClip)
            {
                engineSource.clip = engineAccelerateClip;
                engineSource.loop = true;
                engineSource.Play();
            }
        }
        else
        {
            if (engineSource.clip != engineIdleClip)
            {
                engineSource.clip = engineIdleClip;
                engineSource.loop = true;
                engineSource.Play();
            }
        }
    }

    void PlayBrakeSound()
    {
        if (engineSource != null && brakeClip != null)
        {
            engineSource.PlayOneShot(brakeClip);
        }
    }

    public void TakeDamage(int damage) // Damage
    {
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);
        Debug.Log("HP เหลือ: " + currentHP);
        healthBar.SetHealth(currentHP);

        if (damageSound != null)
                {
                    AudioSource.PlayClipAtPoint(damageSound, transform.position);
                }

        if (currentHP <= 0)
        {
            Debug.Log("Game Over");
            
            rb.linearVelocity = Vector3.zero;
            ShowGameOverPanel();
        }
    }

    public void Heal(int healAmount) // Heal
    {
        currentHP += healAmount;
        currentHP = Mathf.Min(currentHP, maxHP);
        healthBar.SetHealth(currentHP);
        Debug.Log("HP เหลือ: " + currentHP);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Obstacle2 obstacle = other.GetComponent<Obstacle2>();
            TakeDamage(obstacle.damageAmount);
            Destroy(other.gameObject);
        }
        if (other.CompareTag("HealthPickup"))
        {
            HealthPickup healthPickup = other.GetComponent<HealthPickup>();
            Heal(healthPickup.healthAmount);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Goal"))
        {
            SceneManager.LoadScene("Credit");
        }
    }

    // UI
    public int GetCurrentHP()
    {
        return currentHP;
    }

    void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void UpdateTimer()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            ShowTimeOverPanel();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
                timerText.text = "Time Left: " + Mathf.Ceil(currentTime).ToString() + "s";
        }
    }

    void ShowTimeOverPanel()
    {
        timeOverPanel.SetActive(true);
        timeOverPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Time Over!";
        rb.linearVelocity = Vector3.zero;
        GetComponent<CarController>().enabled = false;
    }



}
