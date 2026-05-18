using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Thêm cái này để điều khiển Nút
using TMPro;

public class LevelSelector : MonoBehaviour
{
    [Header("Data Level")]
    public LevelData levelData; 

    [Header("UI States")]
    public GameObject unlockedUI; // Giao diện màn hiện tại (đã mở nhưng chưa chơi xong)
    public GameObject lockedUI;   // Giao diện bị khóa (ổ khóa)
    public GameObject passedUI;   // Giao diện đã vượt qua (có dấu tick hoặc ngôi sao)
    public TextMeshProUGUI coinTextDisplay; // Hiển thị coin ở sảnh

    private bool isUnlocked = false;

    void Start()
    {
        // 1. Kiểm tra tiến độ
        int highestUnlocked = PlayerPrefs.GetInt("HighestUnlockedLevel", 1);

        if (levelData != null)
        {
            // Trạng thái:
            // 1. Passed: Số thứ tự nhỏ hơn màn cao nhất hiện tại
            // 2. Unlocked (Current): Bằng đúng màn cao nhất hiện tại
            // 3. Locked: Lớn hơn màn cao nhất hiện tại

            bool isPassed = (levelData.levelNumber < highestUnlocked);
            isUnlocked = (levelData.levelNumber <= highestUnlocked);
            bool isLocked = !isUnlocked;

            // Bật/Tắt UI tương ứng
            if (passedUI != null) passedUI.SetActive(isPassed);
            if (unlockedUI != null) unlockedUI.SetActive(isUnlocked && !isPassed);
            if (lockedUI != null) lockedUI.SetActive(isLocked);

            // KHÓA CỨNG NÚT BẤM
            var btn = GetComponent<Button>();
            if (btn != null) btn.interactable = isUnlocked;

            Debug.Log("Màn " + levelData.levelNumber + " - Passed: " + isPassed + ", Unlocked: " + isUnlocked);
        }

        // 3. Cập nhật Coin ở sảnh (Đồng bộ tuyệt đối qua file JSON)
        if (coinTextDisplay != null)
        {
            coinTextDisplay.text = InventorySystem.Load().coinCount.ToString();
        }
    }

    public void SelectLevel()
    {
        if (levelData == null) return;

        if (isUnlocked)
        {
            // Nếu đã mở: Vào chơi
            GameManager.selectedLevel = levelData;
            SceneManager.LoadScene("SampleScene"); 
        }
        else
        {
            // Nếu chưa mở: In log thông báo
            Debug.Log("<color=red>Màn " + levelData.levelNumber + " chưa được mở khóa!</color>");
        }
    }
}
