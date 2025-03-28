using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float accelerationForce = 500f;
    public float maxSpeed = 20f;
    public float turnSpeed = 100f;
    public float brakeForce = 1000f;
    public float boostMultiplier = 1.5f; // ค่าคูณเมื่อกด Shift

    [Header("Health Settings")]
    public int maxHP = 10;
    public int currentHP;

    [Header("Audio Settings")]
    public AudioSource engineSource;
    public AudioClip engineIdleClip;
    public AudioClip engineAccelerateClip;
    public AudioClip brakeClip;

    private Rigidbody rb;
    private bool isBraking = false;
    private bool isMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHP = maxHP;

        // ตรวจสอบ AudioSource และเล่นเสียงเครื่องยนต์เริ่มต้น
        if (engineSource != null && engineIdleClip != null)
        {
            engineSource.clip = engineIdleClip;
            engineSource.loop = true;
            engineSource.Play();
        }
    }

    void FixedUpdate()
    {
        ApplyAcceleration();
        ApplySteering();
        ApplyBrakes();
        UpdateEngineSound();
    }

    void ApplyAcceleration()
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

    void ApplySteering()
    {
        float turnInput = Input.GetAxis("Horizontal");
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
            transform.Rotate(0, turn, 0);
        }
    }

    void ApplyBrakes()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity *= 1f - (brakeForce * Time.fixedDeltaTime);
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

    // ฟังก์ชันลด HP เมื่อชนอุปสรรค
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0); // ป้องกัน HP < 0
        Debug.Log("รถชนอุปสรรค! HP เหลือ: " + currentHP);

        if (currentHP <= 0)
        {
            Debug.Log("Game Over! รถพังแล้ว");
            rb.linearVelocity = Vector3.zero; // หยุดการเคลื่อนที่
            rb.isKinematic = true; // ปิดฟิสิกส์ของรถ
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle")) // ตรวจสอบว่าชนอุปสรรคหรือไม่
        {
            TakeDamage(1); // ลด HP 1 หน่วย
            Destroy(other.gameObject); // ลบอุปสรรค
        }
    }

    // ฟังก์ชันเช็ค HP (สำหรับ UI)
    public int GetCurrentHP()
    {
        return currentHP;
    }

    void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Mud")) // พื้นโคลน
    {
        rb.drag = 3f; // เพิ่มแรงเสียดทาน
    }
    else if (collision.gameObject.CompareTag("Sand")) // พื้นทราย
    {
        rb.drag = 2f;
    }
    else if (collision.gameObject.CompareTag("Ice")) // พื้นน้ำแข็ง
    {
        rb.drag = 0.1f; // ลดแรงเสียดทานให้ลื่น
    }
}

void OnCollisionExit(Collision collision)
{
    rb.drag = 0.05f; // กลับไปเป็นค่าเริ่มต้น
}

}
