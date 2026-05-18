using System.Collections.Generic;
using UnityEngine;

public class BattleTeamManager : MonoBehaviour
{
    [Header("UI Slots")]
    public List<BattleSummonButton> summonButtons = new List<BattleSummonButton>();

    [Header("Database Reference")]
    public List<GachaItemSO> allHeroesDatabase = new List<GachaItemSO>();

    void Start()
    {
        InitializeTeamUI();
    }

    public void InitializeTeamUI()
    {
        // 1. Load đội hình đã chọn
        TeamData team = InventorySystem.LoadTeam();
        
        if (team == null || team.selectedHeroIds.Count == 0)
        {
            Debug.LogWarning("BattleTeamManager: Đội hình trống! Hãy chọn tướng trước.");
            return;
        }

        Debug.Log($"BattleTeamManager: Đang chuẩn bị {team.selectedHeroIds.Count} tướng cho trận đấu.");

        // 2. Map ID sang UnitData và hiển thị lên nút
        for (int i = 0; i < summonButtons.Count; i++)
        {
            if (i < team.selectedHeroIds.Count)
            {
                string heroId = team.selectedHeroIds[i];
                // Tìm kiếm không phân biệt hoa thường để tránh lỗi dữ liệu
                GachaItemSO hero = allHeroesDatabase.Find(x => x.id.Equals(heroId, System.StringComparison.OrdinalIgnoreCase));

                if (hero != null && hero.unitData != null)
                {
                    summonButtons[i].gameObject.SetActive(true);
                    summonButtons[i].Setup(hero.unitData, hero.icon);
                }
                else
                {
                    Debug.LogError($"BattleTeamManager: Không tìm thấy dữ liệu cho tướng ID: {heroId}");
                    summonButtons[i].gameObject.SetActive(false);
                }
            }
            else
            {
                // Nếu không có đủ 5 tướng, ẩn các nút thừa
                summonButtons[i].gameObject.SetActive(false);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Load Database")]
    public void LoadDatabaseFromFolder()
    {
        allHeroesDatabase = new List<GachaItemSO>();
        string path = "Assets/itemGaccha";
        string[] guid = UnityEditor.AssetDatabase.FindAssets("t:GachaItemSO", new[] { path });
        foreach (var g in guid)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
            GachaItemSO asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GachaItemSO>(assetPath);
            if (asset != null) allHeroesDatabase.Add(asset);
        }
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"BattleTeamManager: Đã nạp {allHeroesDatabase.Count} tướng vào Database.");
    }
#endif
}
