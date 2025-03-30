using UnityEngine;

public class WallCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody carRb = collision.gameObject.GetComponent<Rigidbody>();
            if (carRb != null)
            {
                carRb.linearVelocity = carRb.linearVelocity * 0.5f;
            }
        }
    }
}
