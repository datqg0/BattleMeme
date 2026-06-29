using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TextMeshProUGUI moneyTextDisplay;
    public TextMeshProUGUI coinTextDisplay;
    public GameObject pauseMenuUI; 
    public GameObject winMenuUI;  
    public GameObject loseMenuUI; 
    public TextMeshProUGUI LevelTextDisplay;
    public Button LevelUp ;

    [Header("Game Settings")]
    public LevelData currentLevel; 

    [Header("Economy")]
    public float currentMoney =0;
    public float currentCoins = 0; 
    float moneyPerSecond = 8f;
    int level = 1;

    [Header("Spawning")]
    public GameObject unitPrefab; 
    public Transform playerSpawnPoint; 
    public Transform enemySpawnPoint;  
    public float spawnYRange = 0.6f;   // Khoảng lệch Y khi spawn (chỉnh trong Inspector)

    [Header("Graphics & Objects")]
    public SpriteRenderer backgroundDisplay; 
    public SpriteRenderer playerBaseRenderer;
    public SpriteRenderer enemyBaseRenderer;
    public AudioSource audioSource; 

    [Header("Button UI")]
    public GameObject resumeButton;    
    public GameObject nextLevelButton; 
    public GameObject restartButton;   
    float mul = 10;

    private bool isPaused = false;
    private int enemiesKilled = 0;    
    private bool bossSpawned = false;
    private int currentEnemyCount = 0;  // Đếm số enemy hiện tại trên bản đồ
    private int currentPlayerCount = 0;  // Đếm số player hiện tại trên bản đồ 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Time.timeScale = 1; 

        // 2. TIỀN TRONG TRẬN BẮT ĐẦU TỪ 0
        currentMoney = 0;
    }

    public static LevelData selectedLevel; 

    void Start()
    {
        LevelUp.image.color = new Color32(255, 0, 0, 100);
        // ĐỒNG BỘ COIN KHI BẮT ĐẦU TRẬN: Lấy từ hệ thống trung tâm
        currentCoins = InventorySystem.Load().coinCount;
        Debug.Log("GameManager: Đã nạp " + currentCoins + " coin từ hệ thống trung tâm.");

        if (selectedLevel != null)
        {
            currentLevel = selectedLevel;
        }

        // Apply level graphics
        if (currentLevel != null)
        {
            if (backgroundDisplay != null && currentLevel.levelBackground != null)
                backgroundDisplay.sprite = currentLevel.levelBackground;

            if (playerBaseRenderer != null && currentLevel.playerBaseSprite != null)
                playerBaseRenderer.sprite = currentLevel.playerBaseSprite;

            if (enemyBaseRenderer != null && currentLevel.enemyBaseSprite != null)
                enemyBaseRenderer.sprite = currentLevel.enemyBaseSprite;

            // PHÁT NHẠC NỀN CỦA MÀN NÀY
            if (audioSource != null && currentLevel.levelBGM != null)
            {
                audioSource.clip = currentLevel.levelBGM;
                audioSource.loop = true; // Nhạc nền nên lặp lại
                audioSource.Play();
            }
        }
    }

    public void LoadLobby()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene("Map1"); 
    }

    void Update()
    {
        if (isPaused) return; 

        currentMoney += moneyPerSecond * Time.deltaTime;
        currentMoney = Math.Min(currentMoney,level*300);
        if (moneyTextDisplay != null)
        {
            moneyTextDisplay.text = Mathf.FloorToInt(currentMoney) + " $ ";
        }

        if (coinTextDisplay != null)
        {
            coinTextDisplay.text = Mathf.FloorToInt(currentCoins) + " ";
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
        int cost = 10 * level * level;
        if (currentMoney>=cost)
        {
            LevelUp.image.color = new Color32(0, 255, 0, 100);
        }
        else
        {
            LevelUp.image.color = new Color32(255, 0, 0, 100);
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0; 
        
        // Hiện bảng Pause, ẩn các bảng khác
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        if (winMenuUI != null) winMenuUI.SetActive(false);
        if (loseMenuUI != null) loseMenuUI.SetActive(false);

        if (resumeButton != null) resumeButton.SetActive(true);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (restartButton != null) restartButton.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1; 
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (winMenuUI != null) winMenuUI.SetActive(false);
        if (loseMenuUI != null) loseMenuUI.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void WinGame()
    {
        int currentLevelNum = currentLevel.levelNumber;
        int highestUnlocked = PlayerPrefs.GetInt("HighestUnlockedLevel", 1);

        if (currentLevelNum >= highestUnlocked)
        {
            PlayerPrefs.SetInt("HighestUnlockedLevel", currentLevelNum + 1);
        }
        
        isPaused = true;
        Time.timeScale = 0;

        // Hiện bảng Thắng, ẩn các bảng khác
        if (winMenuUI != null) winMenuUI.SetActive(true);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (loseMenuUI != null) loseMenuUI.SetActive(false);

        if (resumeButton != null) resumeButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(true);
        if (restartButton != null) restartButton.SetActive(false);

        // ĐỒNG BỘ COIN: Cộng tiền thưởng thắng màn
        if (currentLevel != null)
        {
            AddCoinsToInventory(currentLevel.completionBonus);
        }
    }

    private void AddCoinsToInventory(int amount)
    {
        InventorySystem.AddCoins(amount);
        
        // Cập nhật lại biến hiển thị để UI nhảy số
        currentCoins = InventorySystem.Load().coinCount;
    }
    public void Levelup ()
    {
        int cost = 10 * level * level;

        if(currentMoney>=cost) {
            currentMoney -=cost;
            level++;
            moneyPerSecond += mul/2;
            LevelTextDisplay.text = "Level :" + level;
            LevelUp.image.color = new Color32(255, 0, 0, 100);
            mul=1.35f*mul;
        }
    }
    public void LoseGame()
    {
        isPaused = true;
        Time.timeScale = 0;

        if (loseMenuUI != null) loseMenuUI.SetActive(true);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (winMenuUI != null) winMenuUI.SetActive(false);

        if (resumeButton != null) resumeButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (restartButton != null) restartButton.SetActive(true);
    }

    public void QuitGame()
    {
        LoadLobby();
    }

    public void SpawnPlayerUnit(UnitData unitData)
    {
        // Kiểm tra xem đã đạt giới hạn 25 players chưa
        if (currentPlayerCount >= 20)
        {
            Debug.LogWarning("Đã đạt giới hạn 20 quân!");
            return;
        }
        
        if (currentMoney >= unitData.cost)
        {
            currentMoney -= unitData.cost;
            Vector3 spawnPos = playerSpawnPoint.position + new Vector3(0, UnityEngine.Random.Range(-spawnYRange, spawnYRange), 0);
            
            GameObject newUnit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
            newUnit.name = unitData.unitName;  // Đặt tên đúng trong hierarchy
            newUnit.tag = "Player"; 
            newUnit.layer = LayerMask.NameToLayer("Player"); 

            Unit unitScript = newUnit.GetComponent<Unit>();
            unitScript.data = unitData;
            unitScript.isPlayerUnit = true;
            
            currentPlayerCount++;  // Tăng số lượng player
        }
    }

    public bool TrySpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            return true;
        }
        return false;
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        PlayerPrefs.SetFloat("TotalCoins", currentCoins);
        PlayerPrefs.Save();
    }

    public void SpawnEnemyUnit(UnitData unitData)
    {
        Vector3 spawnPos = enemySpawnPoint.position + new Vector3(0, UnityEngine.Random.Range(-spawnYRange, spawnYRange), 0);

        GameObject newUnit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
        newUnit.name = unitData.unitName;  // Đặt tên đúng trong hierarchy
        newUnit.tag = "Enemy"; 
        newUnit.layer = LayerMask.NameToLayer("Enemy"); 
        SpriteRenderer sr = newUnit.GetComponentInChildren<SpriteRenderer>();
        // Đỏ nhạt (alpha 85%) để vẫn thấy sprite nhưng phân biệt được phe
        if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f, 0.95f);

        Unit unitScript = newUnit.GetComponent<Unit>();
        unitScript.data = unitData;
        unitScript.isPlayerUnit = false;
        
        currentEnemyCount++;  // Tăng số lượng enemy
    }

    public void SpawnBoss()
    {
        if (currentLevel == null || currentLevel.bossUnit == null) return;

        bossSpawned = true;
        Vector3 spawnPos = new Vector3(enemySpawnPoint.position.x, enemySpawnPoint.position.y + 0.5f, 0); // Z=0 đồng nhất
        GameObject bossObj = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
        bossObj.name = currentLevel.bossUnit.unitName;  // Đặt tên đúng cho boss
        
        bossObj.tag = "Enemy"; 
        bossObj.layer = LayerMask.NameToLayer("Enemy"); 
        // KHÔNG nhân scale ở đây nữa! Unit.Start() đã apply data.visualScale rồi
        // → Hãy chỉnh kích thước Boss trong UnitData.visualScale trong Inspector
        SpriteRenderer srBoss = bossObj.GetComponentInChildren<SpriteRenderer>();
        // Boss cũng dùng đỏ nhạt cho đồng nhất với enemy thường
        if (srBoss != null) srBoss.color = new Color(1f, 0.4f, 0.4f, 0.95f);

        Unit bossScript = bossObj.GetComponent<Unit>();
        if (bossScript != null)
        {
            bossScript.data = currentLevel.bossUnit; // Gán dữ liệu con Boss vào đây
            bossScript.isPlayerUnit = false;
            bossScript.isBoss = true; // Đánh dấu đây là Boss
            bossScript.healthBonus = 5f; // Boss trâu hơn 5 lần
        }
        if (currentLevel.bossUnit.bossMusic != null)
        {
            PlayBossMusic(currentLevel.bossUnit.bossMusic);
        }
        Debug.Log("<color=red>CẢNH BÁO: BOSS ĐÃ XUẤT HIỆN!</color>");
    }

    private AudioClip originalMapMusic;

    public void PlayBossMusic(AudioClip bossClip)
    {
        if (audioSource != null && bossClip != null)
        {
            originalMapMusic = audioSource.clip; // Lưu lại nhạc map
            audioSource.Stop();
            audioSource.clip = bossClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void ResumeMapMusic()
    {
        if (audioSource != null && originalMapMusic != null)
        {
            audioSource.Stop();
            audioSource.clip = originalMapMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void PlayNextLevel()
    {
        if (currentLevel != null && currentLevel.nextLevel != null)
        {
            selectedLevel = currentLevel.nextLevel;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            LoadLobby();
        }
    }

    public void RegisterEnemyKilled(bool wasBoss)
    {
        enemiesKilled++;
        currentEnemyCount--;  // Giảm số lượng enemy hiện tại
        
        if (currentLevel != null)
        {
            AddCoinsToInventory(currentLevel.coinRewardPerEnemy);
        }
        if (wasBoss)
        {
            ResumeMapMusic();
            Debug.Log("Boss đã chết! Trả lại nhạc Map.");
        }

        Debug.Log("Đã diệt: " + enemiesKilled + "/" + currentLevel.enemiesToKillForBoss);

        if (!bossSpawned && currentLevel != null && currentLevel.bossUnit != null)
        {
            if (enemiesKilled >= currentLevel.enemiesToKillForBoss)
            {
                SpawnBoss();
                bossSpawned = true;
            }
        }
    }

    public int GetCurrentEnemyCount()
    {
        return currentEnemyCount;
    }

    public int GetCurrentPlayerCount()
    {
        return currentPlayerCount;
    }

    public void DecreasePlayerCount()
    {
        if (currentPlayerCount > 0)
        {
            currentPlayerCount--;
        }
    }
}
