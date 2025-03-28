using UnityEngine;

public class Obstacle2 : MonoBehaviour
{
    public int damageAmount = 1; // จำนวนเลือดที่ลด

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // ตรวจสอบว่าชนกับรถหรือไม่
        {
            CarController car = other.GetComponent<CarController>();

            if (car != null)
            {
                car.TakeDamage(damageAmount); // ลด HP ของรถ
                Destroy(gameObject); // ทำลายอุปสรรค
            }
        }
    }
}
