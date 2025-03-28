using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float fallSpeed = 10f; // ความเร็วตกเริ่มต้น
    public float speedMultiplier = 1.2f; // ความเร็วเพิ่มขึ้นเมื่อเวลาผ่านไป
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // ปิด Gravity ของ Unity เพราะเราจะใช้ AddForce แทน
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * fallSpeed, ForceMode.Acceleration); // ให้ตกลงมาแบบฟิสิกส์
        fallSpeed *= speedMultiplier * Time.fixedDeltaTime; // ทำให้ตกเร็วขึ้นเรื่อยๆ
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // ชนรถ
        {
            other.GetComponent<CarController>().TakeDamage(1); // ลด HP
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground")) // ชนพื้น
        {
            Destroy(gameObject);
        }
    }
}
