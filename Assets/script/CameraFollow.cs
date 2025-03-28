using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;  // รถที่กล้องจะตาม
    public Vector3 offset = new Vector3(0, 5, -10);  // ตำแหน่งของกล้องด้านหลังรถ
    public float followSpeed = 10f;  // ความเร็วในการตาม
    public float rotationSpeed = 5f;  // ความเร็วในการหมุน

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // คำนวณตำแหน่งใหม่ของกล้อง
        Vector3 targetPosition = target.position + target.TransformDirection(offset);

        // ใช้ SmoothDamp เพื่อให้กล้องเคลื่อนที่นุ่มนวล
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed);

        // หมุนกล้องให้หันไปทางรถอย่างนุ่มนวล
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
