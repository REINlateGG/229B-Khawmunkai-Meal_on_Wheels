using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // ฟังก์ชันเริ่มเกม
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene"); // ใส่ชื่อ Scene ของเกมจริง
    }

    // ฟังก์ชันออกจากเกม
    public void ExitGame()
    {
        Debug.Log("ออกจากเกม"); // สำหรับทดสอบใน Unity Editor
        Application.Quit();
    }
}
