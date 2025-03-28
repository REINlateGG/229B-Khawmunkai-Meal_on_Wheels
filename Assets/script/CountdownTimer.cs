using UnityEngine;
using UnityEngine.UI;
using TMPro; // ต้องใช้ TextMeshPro สำหรับ UI

public class CountdownTimer : MonoBehaviour
{
    public float timeRemaining = 180f; // 3 นาที (180 วินาที)
    public TextMeshProUGUI timerText;
    private bool isGameOver = false;

    void Update()
    {
        if (isGameOver) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            timeRemaining = 0;
            GameOver(); // เรียกฟังก์ชันเมื่อเวลาหมด
        }
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds); // แสดงเป็น MM:SS
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("หมดเวลา! แพ้แล้ว 😢");
        // TODO: เพิ่มระบบแพ้ เช่น เปลี่ยนซีน หรือแสดง UI Game Over
    }
}
