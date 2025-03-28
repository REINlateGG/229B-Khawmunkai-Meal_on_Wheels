using UnityEngine;
using System.Collections;

public class CarGravity : MonoBehaviour
{
    public Rigidbody rb;
    public float escapeForce = 2000f; // แรงที่ใช้หนีออกจากแรงโน้มถ่วง
    public float minDistanceToPlanet = 3f; // ระยะที่เริ่มโดนดูด
    public int damagePerSecond = 1; // จำนวน HP ที่ลดลงต่อวินาที
    private bool isTakingDamage = false; // ตรวจสอบว่ากำลังโดนดูดหรือไม่

    private CarController carController; // อ้างอิงไปที่ HP ของรถ

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>(); // ดึง CarController มาใช้
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Planet"))
        {
            Debug.Log("รถเข้าใกล้ดาวเคราะห์! เริ่มถูกดูด");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Planet"))
        {
            // คำนวณแรงโน้มถ่วง
            Vector3 gravityDirection = (other.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, other.transform.position);

            // ถ้าอยู่ใกล้เกิน minDistanceToPlanet → เริ่มลด HP
            if (distance < minDistanceToPlanet)
            {
                if (!isTakingDamage)
                {
                    StartCoroutine(DamageOverTime());
                    isTakingDamage = true;
                }
            }
            else
            {
                isTakingDamage = false;
                StopCoroutine(DamageOverTime());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Planet"))
        {
            Debug.Log("✅ รถหนีออกจากดาวเคราะห์แล้ว!");
            isTakingDamage = false;
            StopCoroutine(DamageOverTime());
        }
    }

    IEnumerator DamageOverTime()
    {
        while (isTakingDamage && carController.currentHP > 0)
        {
            carController.TakeDamage(damagePerSecond);
            Debug.Log(" รถกำลังถูกดูด! HP: " + carController.currentHP);
            yield return new WaitForSeconds(1f); // ลด HP ทุก 1 วินาที
        }

        if (carController.currentHP <= 0)
        {
            Debug.Log(" Game Over! รถถูกดูดจนพัง!");
        }
    }

    public void EscapeGravity()
    {
        rb.AddForce(-transform.up * escapeForce, ForceMode.Impulse);
        Debug.Log(" รถใช้พลังหนีออกจากแรงโน้มถ่วง!");
    }
}
