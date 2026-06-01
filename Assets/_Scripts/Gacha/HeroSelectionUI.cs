using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Tilemaps;

public class HeroSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform spawnPoint;
    public GameObject selectionPanel;
    public Transform contentParent;
    public GameObject heroSlotPrefab;
    public Button confirmButton;
    public TextMeshProUGUI selectionCountText; // Ví dụ: "Đã chọn: 3/5"
    public TextMeshProUGUI StoreInformationText;
    public GameObject spawnhero; 
    [Header("Settings")]
    public string mainMenuSceneName = "MainMenuScene"; 

    [Header("Database")]
    public List<GachaItemSO> itemDatabase = new List<GachaItemSO>();

    private PlayerInventory playerInventory = new PlayerInventory();
    private TeamData teamData = new TeamData();
    private List<string> tempSelectedIds = new List<string>(); 
    private List<HeroSelectionSlot> spawnedSlots = new List<HeroSelectionSlot>();
    
    void Start()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
        OpenSelection();
        HeroSelectionManager.Instance.Setup(itemDatabase,spawnPoint,StoreInformationText);
    }

    public void OpenSelection()
    {
        if (selectionPanel != null) selectionPanel.SetActive(true);
        
        LoadData();
        PopulateList();
    }

    private void LoadData()
    {
        playerInventory = InventorySystem.Load();
        teamData = InventorySystem.LoadTeam();
        
        // Filter: Chỉ lấy những ID thực sự tồn tại trong Database của UI này (Không phân biệt hoa thường)
        tempSelectedIds = new List<string>();
        foreach (string id in teamData.selectedHeroIds)
        {
            var matchedItem = itemDatabase.Find(x => x.id.Equals(id, System.StringComparison.OrdinalIgnoreCase));
            if (matchedItem != null)
            {
                // Lấy ID chuẩn từ Database để đảm bảo đồng bộ tuyệt đối
                tempSelectedIds.Add(matchedItem.id);
            }
        }
        
        Debug.Log($"HeroSelectionUI: Đã nạp {tempSelectedIds.Count} tướng hợp lệ từ Database.");
    }

    public void PopulateList()
    {
        Debug.Log("<color=cyan>HeroSelectionUI: Bắt đầu tạo danh sách tướng...</color>");
        
        foreach (var slot in spawnedSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        spawnedSlots.Clear();

        if (playerInventory.ownedCharacterIds.Count < 1)
        {
            Debug.LogWarning("HeroSelectionUI: Bạn chưa có đủ 5 tướng để lập đội hình! Hãy vào Gacha để nhận thêm.");
            // Có thể hiện UI thông báo tại đây nếu muốn
        }

        if (playerInventory.ownedCharacterIds.Count == 0)
        {
            Debug.LogWarning("HeroSelectionUI: Inventory trống rỗng!");
            return;
        }

        foreach (string id in playerInventory.ownedCharacterIds)
        {
            // Tìm tướng trong database (Không phân biệt hoa thường)
            GachaItemSO item = itemDatabase.Find(x => x.id.Equals(id, System.StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                Debug.LogWarning($"HeroSelectionUI: ID '{id}' không có trong Database!");
                continue;
            }

            if (heroSlotPrefab == null)
            {
                Debug.LogError("HeroSelectionUI: BẠN CHƯA KÉO PREFAB VÀO Ô 'Hero Slot Prefab'!");
                return;
            }

            GameObject go = Instantiate(heroSlotPrefab, contentParent);
            HeroSelectionSlot slot = go.GetComponent<HeroSelectionSlot>();
            
            if (slot == null)
            {
                Debug.LogError($"HeroSelectionUI: Prefab '{heroSlotPrefab.name}' THIẾU script 'HeroSelectionSlot'!");
                Destroy(go);
                continue;
            }

            // Setup
            slot.Setup(item, this, tempSelectedIds.Contains(item.id));
            spawnedSlots.Add(slot);
        }
        
        UpdateConfirmButtonState();
        Debug.Log($"HeroSelectionUI: Đã hiển thị {spawnedSlots.Count} tướng.");
    }

    private void UpdateSelectionVisuals()
    {
        foreach (var slot in spawnedSlots)
        {
            if (slot != null)
            {
                slot.SetSelected(tempSelectedIds.Contains(slot.HeroId));
            }
        }
        UpdateConfirmButtonState();
    }

    private void UpdateConfirmButtonState()
    {
        int count = tempSelectedIds.Count;
        bool isFull = (count == 5);

        if (confirmButton != null)
        {
            confirmButton.interactable = isFull;
            
            // Đổi màu nút để biết đã đủ 5 con chưa
            Image btnImg = confirmButton.GetComponent<Image>();
            if (btnImg != null) btnImg.color = isFull ? Color.green : Color.white;
        }

        if (selectionCountText != null)
        {
            selectionCountText.text = $"Đã chọn: <color={(isFull ? "green" : "yellow")}>{count}/5</color> Tướng";
            if (isFull) selectionCountText.text += " (ĐỦ ĐIỀU KIỆN!)";
        }
    }

    public void SetTemporarySelection(string id)
    {
        if (tempSelectedIds.Contains(id))
        {
            tempSelectedIds.Remove(id);
        }
        else
        {
            if (tempSelectedIds.Count < 5)
            {
                tempSelectedIds.Add(id);
            }
            else
            {
                Debug.LogWarning("HeroSelectionUI: Đã chọn tối đa 5 tướng!");
                return; 
            }
        }
        UpdateSelectionVisuals();
    }

    public void ConfirmSelection()
    {
        //if (tempSelectedIds.Count != 5) return;

        teamData.selectedHeroIds = new List<string>(tempSelectedIds);
        InventorySystem.SaveTeam(teamData);

        Debug.Log("HeroSelectionUI: Đội hình đã được lưu vào team.json!");
        
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void ClearSelection()
    {
        tempSelectedIds.Clear();
        UpdateSelectionVisuals();
        Debug.Log("HeroSelectionUI: Đã xóa toàn bộ lựa chọn.");
    }

    
#if UNITY_EDITOR
    [ContextMenu("Load Database From Folder")]
    public void LoadDatabaseFromFolder()
    {
        itemDatabase = new List<GachaItemSO>();
        string path = "Assets/itemGaccha";
        string[] guid = UnityEditor.AssetDatabase.FindAssets("t:GachaItemSO", new[] { path });
        foreach (var g in guid)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
            GachaItemSO asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GachaItemSO>(assetPath);
            if (asset != null) itemDatabase.Add(asset);
        }
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"HeroSelectionUI: Đã tự động load {itemDatabase.Count} tướng vào database riêng!");
    }
#endif
}
