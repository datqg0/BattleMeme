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
    float moneyPerSecond = 7f;
    int level = 1;

    [Header("Spawning")]
    public GameObject unitPrefab; 
    public Transform playerSpawnPoint; 
    public Transform enemySpawnPoint;  

    [Header("Graphics & Objects")]
    public SpriteRenderer backgroundDisplay; 
    public SpriteRenderer playerBaseRenderer;
    public SpriteRenderer enemyBaseRenderer;
    public AudioSource audioSource; 

    [Header("Button UI")]
    public GameObject resumeButton;    
    public GameObject nextLevelButton; 
    public GameObject restartButton;   
    int mul = 10;

    private bool isPaused = false;
    private int enemiesKilled = 0;    
    private bool bossSpawned = false; 

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
        SceneManager.LoadScene("LoppyScene"); 
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
        int cost = mul * level;
        if (currentMoney>=cost)
        {
            LevelUp.image.color = new Color32(0, 255, 0, 100);
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
        // Sử dụng hệ thống trung tâm để cộng tiền (An toàn tuyệt đối)
        InventorySystem.AddCoins(amount);
        
        // Cập nhật lại biến hiển thị để UI nhảy số
        currentCoins = InventorySystem.Load().coinCount;
        
        Debug.Log("Đã nạp " + amount + " coin. Tổng mới: " + currentCoins);
    }
    public void Levelup ()
    {
        int cost = mul * level;

        if(currentMoney>=cost) {
            currentMoney -=cost;
            level++;
            moneyPerSecond += mul/2;
            LevelTextDisplay.text = "Level :" + level;
            LevelUp.image.color = new Color32(255, 0, 0, 100);
            mul += 2*mul;
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
        if (currentMoney >= unitData.cost)
        {
            currentMoney -= unitData.cost;
            Vector3 spawnPos = playerSpawnPoint.position + new Vector3(0, UnityEngine.Random.Range(-0.3f, 0.3f), 0);
            
            GameObject newUnit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
            newUnit.tag = "Player"; 
            newUnit.layer = LayerMask.NameToLayer("Player"); 

            Unit unitScript = newUnit.GetComponent<Unit>();
            unitScript.data = unitData;
            unitScript.isPlayerUnit = true; 
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
        Vector3 spawnPos = enemySpawnPoint.position + new Vector3(0, UnityEngine.Random.Range(-0.3f, 0.3f), 0);

        GameObject newUnit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
        newUnit.tag = "Enemy"; 
        newUnit.layer = LayerMask.NameToLayer("Enemy"); 
        SpriteRenderer sr = newUnit.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = Color.red;

        Unit unitScript = newUnit.GetComponent<Unit>();
        unitScript.data = unitData;
        unitScript.isPlayerUnit = false; 
    }

    public void SpawnBoss()
    {
        if (currentLevel == null || currentLevel.bossUnit == null) return;

        bossSpawned = true;
        Vector3 spawnPos = new Vector3(enemySpawnPoint.position.x, enemySpawnPoint.position.y + 0.5f, 0);
        GameObject bossObj = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
        
        bossObj.tag = "Enemy"; 
        bossObj.layer = LayerMask.NameToLayer("Enemy"); 
        bossObj.transform.localScale *= 3f;
        SpriteRenderer srBoss = bossObj.GetComponentInChildren<SpriteRenderer>();
        if (srBoss != null) srBoss.color = Color.red;

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
}
