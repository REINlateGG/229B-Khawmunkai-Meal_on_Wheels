using UnityEngine;

public class Obstacle2 : MonoBehaviour
{
    public int damageAmount = 1;
    public float speedReduction = 0.5f;
    public AudioClip damageSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CarController car = other.GetComponent<CarController>();

            if (car != null)
            {
                car.TakeDamage(damageAmount);

                Rigidbody carRb = car.GetComponent<Rigidbody>();
                if (carRb != null)
                {
                    carRb.linearVelocity *= speedReduction;
                }
                if (damageSound != null)
                {
                    AudioSource.PlayClipAtPoint(damageSound, transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
    
}
