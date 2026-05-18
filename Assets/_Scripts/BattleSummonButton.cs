using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleSummonButton : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI costText;
    public Image cooldownOverlay; // Image kiểu Filled để làm hiệu ứng hồi chiêu
    public Button summonButton;

    [Header("Manual Setup (Dành cho nút cố định/Boss)")]
    public UnitData manualUnitData;
    public Sprite manualIcon;

    private UnitData unitData;
    private float currentCooldown = 0;
    private bool isInitialized = false;

    void Start()
    {
        // Nếu bạn kéo tay UnitData vào Inspector, nó sẽ tự setup luôn mà không đợi TeamManager
        if (manualUnitData != null)
        {
            Setup(manualUnitData, manualIcon);
        }
    }

    public void Setup(UnitData data, Sprite icon)
    {
        unitData = data;
        if (iconImage != null) iconImage.sprite = icon;
        if (costText != null) costText.text = data.cost.ToString() + "$";
        
        isInitialized = true;
        if (summonButton != null) summonButton.interactable = true;
    }

    void Update()
    {
        if (!isInitialized || unitData == null) return;

        // 1. Xử lý Cooldown
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = currentCooldown / unitData.spawnCooldown;
            }
        }
        else
        {
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0;
        }

        // 2. Kiểm tra điều kiện bấm nút (Tiền và Cooldown)
        bool canAfford = GameManager.Instance != null && GameManager.Instance.currentMoney >= unitData.cost;
        bool isReady = currentCooldown <= 0;

        if (summonButton != null)
        {
            summonButton.interactable = canAfford && isReady;
        }
    }

    public void OnClickSummon()
    {
        if (unitData == null) return;

        if (GameManager.Instance != null && currentCooldown <= 0)
        {
            // Kiểm tra lại tiền lần cuối trong GameManager
            if (GameManager.Instance.currentMoney >= unitData.cost)
            {
                GameManager.Instance.SpawnPlayerUnit(unitData);
                currentCooldown = unitData.spawnCooldown;
            }
        }
    }
}
