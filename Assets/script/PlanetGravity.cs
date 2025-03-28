using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GravityPlanet : MonoBehaviour
{
    public float gravitationalConstant = 0.01f; // ✅ ค่าคงที่แรงโน้มถ่วง (G)
    public float damageDistance = 3f; // ✅ ระยะที่เริ่มลด HP
    public int damagePerSecond = 1; // ✅ จำนวน HP ที่ลดต่อวินาที

    private List<Rigidbody> affectedObjects = new List<Rigidbody>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && !affectedObjects.Contains(rb))
            {
                affectedObjects.Add(rb);
                StartCoroutine(DamageOverTime(other.GetComponent<CarController>()));
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && affectedObjects.Contains(rb))
            {
                affectedObjects.Remove(rb);
            }
        }
    }

    void FixedUpdate()
    {
        foreach (Rigidbody rb in affectedObjects)
        {
            if (rb == null) continue;

            Vector3 direction = (transform.position - rb.position);
            float distance = direction.magnitude;

            if (distance > 0.5f) // ✅ ป้องกัน error แรงโน้มถ่วงเป็น infinity
            {
                float forceMagnitude = gravitationalConstant * (rb.mass * 1000f) / Mathf.Pow(distance, 2);
                rb.AddForce(direction.normalized * forceMagnitude, ForceMode.Acceleration);
            }
        }
    }

    IEnumerator DamageOverTime(CarController car)
    {
        while (car != null && affectedObjects.Contains(car.GetComponent<Rigidbody>()))
        {
            float distance = Vector3.Distance(transform.position, car.transform.position);
            if (distance < damageDistance)
            {
                car.TakeDamage(damagePerSecond);
                Debug.Log("🔥 รถใกล้ดาวเกินไป! HP เหลือ: " + car.currentHP);
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
