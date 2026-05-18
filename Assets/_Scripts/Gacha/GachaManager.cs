using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance;

    private string poolFilePath;
    private string inventoryFilePath;

    public GachaPool currentPool = new GachaPool();
    public PlayerInventory playerInventory = new PlayerInventory();

    [Header("Asset Database (Khoa học)")]
    public List<GachaItemSO> itemDatabase; // Kéo tất cả ScriptableObject vào đây

    public int ticketPrice = 100; // Giá 1 vé là 100 coin

    public GameObject GetPrefabById(string id)
    {
        var item = itemDatabase.Find(x => x.id == id);
        return item != null ? item.prefab : null;
    }

    private void Awake()
    {
        // Singleton đơn giản cho mỗi Scene: Khi chuyển cảnh, Instance cũ bị hủy, Instance mới ở Scene mới sẽ được gán.
        Instance = this;

        poolFilePath = Path.Combine(Application.persistentDataPath, "gacha_pool.json");
        inventoryFilePath = Path.Combine(Application.persistentDataPath, "inventory.json");

        LoadPool();
        LoadInventory();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void LoadPool()
    {
        if (File.Exists(poolFilePath))
        {
            string json = File.ReadAllText(poolFilePath);
            currentPool = JsonUtility.FromJson<GachaPool>(json);
            Debug.Log("Gacha Pool Loaded from: " + poolFilePath);
        }
        else
        {
            // TỰ ĐỘNG TẠO POOL THEO ĐỊNH DẠNG YÊU CẦU
            currentPool = new GachaPool();
            
            // 1. Thêm tiền và vé
            currentPool.items.Add(new GachaItem { id = "coin_50", characterName = "50 Coins", rarity = "Common", rate = 0.3f, isCurrency = true, coinReward = 50 });
            currentPool.items.Add(new GachaItem { id = "coin_100", characterName = "100 Coins", rarity = "Rare", rate = 0.15f, isCurrency = true, coinReward = 100 });
            currentPool.items.Add(new GachaItem { id = "ticket_1", characterName = "1 Gacha Ticket", rarity = "Rare", rate = 0.1f, isCurrency = true, ticketReward = 1 });

            // 2. Thêm tướng với ID tùy chỉnh
            AddHeroToPool("domixi", "DoMixi(unit_template)");
            AddHeroToPool("pepe", "Pepe(unit_template)");
            AddHeroToPool("shiba", "Shiba(unit_template)");
            AddHeroToPool("sigmaman", "SigmaMan(unit_template) 3");
            AddHeroToPool("bitchplese", "bitchplese(unit_template)");

            string[] simpleHeroes = { 
                "cappypara", "coffindance", "ishowspeed", "mu", "pandameme", 
                "penguin", "pewpew", "pooh", "ricadomilos", "saltedeggs", "shiba2", "shrek", 
                "simson", "slenderman", "steave", "therock", "tomlizard", 
                "trollface1", "trollface2", "trollface3", "yunoman" 
            };

            foreach (var h in simpleHeroes) {
                AddHeroToPool(h, h);
            }

            SavePool();
            Debug.Log("Created comprehensive gacha_pool.json with custom IDs at: " + poolFilePath);
        }
    }

    private void AddHeroToPool(string id, string prefabName) {
        string[] rarities = { "Common", "Rare", "SR", "SSR" };
        float[] weights = { 40f, 30f, 20f, 10f }; // Tỷ lệ xuất hiện của bậc hiếm đó trong pool
        
        // Chọn ngẫu nhiên Rarity dựa trên trọng số
        float totalWeight = weights.Sum();
        float randomValue = Random.Range(0, totalWeight);
        string selectedRarity = "Common";
        
        float currentSum = 0;
        for (int i = 0; i < weights.Length; i++) {
            currentSum += weights[i];
            if (randomValue <= currentSum) {
                selectedRarity = rarities[i];
                break;
            }
        }

        // Gán Rate (tỷ lệ trúng) tương ứng với bậc hiếm
        float rate = 0.05f;
        switch (selectedRarity) {
            case "Common": rate = 0.05f; break;
            case "Rare": rate = 0.03f; break;
            case "SR": rate = 0.015f; break;
            case "SSR": rate = 0.005f; break;
        }

        currentPool.items.Add(new GachaItem { 
            id = id, 
            characterName = prefabName, 
            rarity = selectedRarity, 
            rate = rate, 
            prefabPath = "Heroes/" + prefabName,
            isCurrency = false
        });
    }

    public void SavePool()
    {
        string json = JsonUtility.ToJson(currentPool, true);
        File.WriteAllText(poolFilePath, json);
        Debug.Log("Gacha Pool Saved to: " + poolFilePath);
    }

    public void LoadInventory()
    {
        playerInventory = InventorySystem.Load();
    }

    public void SaveInventory()
    {
        if (playerInventory == null) return;
        InventorySystem.Save(playerInventory);
        Debug.Log("<color=green>Inventory Saved Successfully!</color>");
    }

    // Hàm cưỡng bức nạp lại dữ liệu sạch
    public void RefreshData()
    {
        playerInventory = InventorySystem.Load();
        Debug.Log("<color=cyan>GachaManager: Data Refreshed from Disk.</color>");
    }

    public bool BuyTicket()
    {
        if (playerInventory.coinCount >= ticketPrice)
        {
            playerInventory.coinCount -= ticketPrice;
            playerInventory.ticketCount++;
            SaveInventory();
            return true;
        }
        return false;
    }

    public GachaItem Roll()
    {
        if (playerInventory.ticketCount <= 0)
        {
            Debug.LogWarning("Không đủ vé để quay!");
            return null;
        }

        // Lọc danh sách: Chỉ lấy những item KHÔNG phải tướng ĐÃ sở hữu
        var availableItems = currentPool.items.Where(item => 
            item.isCurrency || !playerInventory.ownedCharacterIds.Contains(item.id)
        ).ToList();

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("Bạn đã sở hữu tất cả tướng! Chỉ còn lại phần thưởng tiền.");
            availableItems = currentPool.items.Where(item => item.isCurrency).ToList();
        }

        if (availableItems.Count == 0) return null;

        // Trừ vé
        playerInventory.ticketCount--;

        float totalRate = availableItems.Sum(item => item.rate);
        float randomPoint = Random.Range(0, totalRate);

        foreach (var item in availableItems)
        {
            if (randomPoint < item.rate)
            {
                if (item.isCurrency)
                {
                    playerInventory.coinCount += item.coinReward;
                    playerInventory.ticketCount += item.ticketReward;
                    Debug.Log("Nhận thưởng: " + item.characterName);
                }
                else
                {
                    AddCharacterToInventory(item.id);
                }
                
                SaveInventory();
                return item;
            }
            randomPoint -= item.rate;
        }

        return availableItems.Last();
    }

    private void AddCharacterToInventory(string id)
    {
        if (!playerInventory.ownedCharacterIds.Contains(id))
        {
            playerInventory.ownedCharacterIds.Add(id);
            SaveInventory();
        }
    }
}
