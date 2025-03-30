using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public TextMeshProUGUI speedText;

    void Update()
    {
        if (carRigidbody != null && speedText != null)
        {
            float speed = carRigidbody.linearVelocity.magnitude * 3.6f;
            speedText.text = "Speed: " + speed.ToString("F0") + " km/h";
        }
    }
}