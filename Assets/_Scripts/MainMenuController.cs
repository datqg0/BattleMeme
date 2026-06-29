using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            ResetGameData();
        }
    }

    // Hàm này dành riêng cho Nút bấm (Button) trong UI
    public void ResetGameData()
    {
        Debug.Log("<color=yellow>Nút RESET đã được bấm!</color>");
        InventorySystem.ClearInventory();
        
        // Tải lại cảnh hiện tại để cập nhật giao diện ngay lập tức
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
        Debug.Log("Đang tải lại cảnh: " + currentScene);
    }

    // Hàm này để nhấn nút PLAY sẽ chuyển sang Sảnh chọn màn
    public void PlayGame()
    {
        SceneManager.LoadScene("Map1"); 
    }

    // Hàm này để thoát khỏi màn hình Gacha quay về Sảnh
    public void ExitGacha()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
    public void GoToGacha()
    {
        Debug.Log("Chuyển sang Gaccha Scene...");
        SceneManager.LoadScene("Gaccha"); 
    }
    // Hàm này để chuyển sang màn hình Chọn Tướng (PickHero)
    public void GoToPickHero()
    {
        Debug.Log("Chuyển sang PickHero Scene...");
        SceneManager.LoadScene("PickHero");
    }

    // Hàm này để nhấn nút THOÁT sẽ đóng hẳn game
    public void QuitApp()
    {
        Debug.Log("Đã đóng game!");
        Application.Quit();
    }
    
}
