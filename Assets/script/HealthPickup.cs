using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healthAmount = 5;
    public AudioClip pickupSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CarController car = other.GetComponent<CarController>();

            if (car != null)
            {
                car.Heal(healthAmount);

                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
}
