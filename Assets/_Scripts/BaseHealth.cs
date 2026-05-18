using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseHealth : MonoBehaviour
{
    public bool isPlayerBase;
    public float maxHealth = 1000f;
    private float currentHealth;

    [Header("UI Base Health")]
    public Slider hpSlider;         // Kéo Slider vào đây
    public TextMeshProUGUI hpText;  // Kéo Text vào đây

    void Start()
    {
        // 1. Thiết lập máu tối đa (Nếu là địch thì nhân theo hệ số level)
        if (!isPlayerBase && GameManager.Instance != null)
        {
            maxHealth *= GameManager.Instance.currentLevel.enemyHealthMultiplier;
        }
        
        currentHealth = maxHealth;

        // 2. Cập nhật giao diện ban đầu
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return; // Nếu đã chết thì không nhận sát thương nữa

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void UpdateUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        if (hpText != null)
        {
            hpText.text = Mathf.CeilToInt(currentHealth) + " / " + Mathf.CeilToInt(maxHealth);
        }
    }

    void GameOver()
    {
        if (GameManager.Instance != null)
        {
            if (isPlayerBase)
            {
                Debug.Log("THẤT BẠI! Nhà chính đã bị phá hủy.");
                GameManager.Instance.LoseGame(); // Gọi hàm Thua cuộc mới
            }
            else
            {
                Debug.Log("CHIẾN THẮNG! Bạn đã phá hủy căn cứ địch.");
                GameManager.Instance.WinGame(); 
            }
        }
    }
}
