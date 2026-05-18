using UnityEngine;
using TMPro;

public class GachaDisplayUI : MonoBehaviour
{
    public Transform spawnPoint; // Chỗ để hiện tướng
    public TextMeshProUGUI resultNameText;
    public GameObject resultPanel; // Panel hiện kết quả

    [Header("Currency UI")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI ticketText;
    public TextMeshProUGUI ownedCountText;

    private GameObject currentSpawnedHero;

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (GachaManager.Instance == null) return;
        
        var inv = GachaManager.Instance.playerInventory;
        if (coinText != null) coinText.text = ":" + inv.coinCount;
        if (ticketText != null) ticketText.text = ": " + inv.ticketCount;
        if (ownedCountText != null) ownedCountText.text = "Owned: " + inv.ownedCharacterIds.Count;
    }

    public void OnRollButtonClicked()
    {
        GachaItem result = GachaManager.Instance.Roll();

        if (result != null)
        {
            DisplayResult(result);
            UpdateUI();
        }
        else
        {
            if (GachaManager.Instance.playerInventory.ticketCount <= 0)
            {
                Debug.LogError("Bạn không có đủ vé để quay!");
            }
            else
            {
                Debug.LogError("Pool đang trống! Vui lòng thêm tướng trong Admin UI.");
            }
        }
    }

    public void OnBuyTicketButtonClicked()
    {
        if (GachaManager.Instance.BuyTicket())
        {
            UpdateUI();
            Debug.Log("Mua vé thành công!");
        }
        else
        {
            Debug.LogError("Không đủ Coin để mua vé!");
        }
    }

    private void DisplayResult(GachaItem item)
    {
        if (currentSpawnedHero != null) Destroy(currentSpawnedHero);

        if (!item.isCurrency)
        {
            GameObject prefab = GachaManager.Instance.GetPrefabById(item.id);
            
            if (prefab != null)
            {
                currentSpawnedHero = Instantiate(prefab, spawnPoint);
                
                // NGĂN DI CHUYỂN: Tắt các script điều khiển và vật lý
                Unit unitScript = currentSpawnedHero.GetComponent<Unit>();
                if (unitScript != null) {
                    unitScript.enabled = false;
                    // Tắt thanh máu cho đẹp
                    if (unitScript.hpSliderObject != null) unitScript.hpSliderObject.SetActive(false);
                }

                Rigidbody2D rb = currentSpawnedHero.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = false;

                // Reset vị trí (đẩy Z về phía trước để hiện trên UI) và scale
                currentSpawnedHero.transform.localPosition = new Vector3(0, 0, -10f);
                currentSpawnedHero.transform.localScale = Vector3.one * 80f; 

                // Nếu là game 2D, đảm bảo Sorting Order cao để hiện lên trên
                SpriteRenderer[] srs = currentSpawnedHero.GetComponentsInChildren<SpriteRenderer>();
                foreach(var sr in srs) {
                    sr.sortingOrder = 100; 
                }

                Debug.Log("Spawn thành công tướng: " + item.characterName);
            }
            else
            {
                Debug.LogError("KHÔNG TÌM THẤY PREFAB: Resources/" + item.prefabPath);
            }
            
            resultNameText.text = "BẠN ĐÃ NHẬN ĐƯỢC TƯỚNG: " + item.characterName + " (" + item.rarity + ")";
        }
        else
        {
            // Nếu là tiền/vé
            resultNameText.text = "PHẦN THƯỞNG: " + item.characterName;
        }

        resultPanel.SetActive(true);
    }

    public void CloseResult()
    {
        resultPanel.SetActive(false);
        if (currentSpawnedHero != null) Destroy(currentSpawnedHero);
    }
}
