using UnityEngine;
using TMPro; // ใช้ TextMeshPro

public class WinGame : MonoBehaviour
{
    public GameObject winUI; // UI ชนะเกม
    public CountdownTimer countdownTimer; // อ้างถึงตัวจับเวลา
    private bool gameWon = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal") && !gameWon)
        {
            gameWon = true;
            Win();
        }
    }

    void Win()
    {
        Debug.Log("🎉 ชนะเกม! คุณไปถึงจุดหมายแล้ว!");
        winUI.SetActive(true); // เปิด UI ชนะเกม
        countdownTimer.enabled = false; // หยุดจับเวลา

        // หยุดการควบคุมรถ (ปิดการทำงานของ CarController)
        GetComponent<CarController>().enabled = false;
    }
}
