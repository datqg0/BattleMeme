using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class InventorySystem
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "inventory.json");
    private static string TeamFilePath => Path.Combine(Application.persistentDataPath, "team.json");

    public static PlayerInventory Load()
    {
        PlayerInventory inv = new PlayerInventory();
        
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            inv = JsonUtility.FromJson<PlayerInventory>(json);
        }
        else
        {
            // Thiết lập mặc định cho tài khoản mới
            inv.coinCount = 1000;
            inv.ticketCount = 5;
        }
        
        // --- CHẾ ĐỘ BẢO TRÌ DỮ LIỆU (SENIOR LOGIC) ---
        // Sử dụng chính xác ID từ Database của bạn để tránh lỗi "Không tìm thấy dữ liệu"
        string[] starters = { "domixi", "pepe", "shiba", "SigmaMan(unit_template) 3", "bitchplese(unit_template)" };
        bool hasChanged = false;

        foreach (string id in starters)
        {
            if (!inv.ownedCharacterIds.Contains(id))
            {
                inv.ownedCharacterIds.Add(id);
                hasChanged = true;
            }
        }

        // Nếu có sự thay đổi (thêm tướng mới), lưu lại ngay
        if (hasChanged || !File.Exists(FilePath))
        {
            Save(inv);
            
            // Nếu là lần đầu hoặc dữ liệu bị trống, thiết lập luôn đội hình mặc định
            TeamData team = LoadTeam();
            if (team.selectedHeroIds.Count == 0)
            {
                team.selectedHeroIds = new List<string>(starters);
                SaveTeam(team);
            }
        }

        return inv;
    }

    public static void Save(PlayerInventory inventory)
    {
        string json = JsonUtility.ToJson(inventory, true);
        File.WriteAllText(FilePath, json);
    }

    public static void AddCoins(int amount)
    {
        PlayerInventory inv = Load();
        inv.coinCount += amount;
        Save(inv);
        
        // Nếu GachaManager đang chạy, cập nhật nó luôn
        if (GachaManager.Instance != null)
        {
            GachaManager.Instance.playerInventory = inv;
        }
    }

    public static void ClearInventory()
    {
        Debug.Log("--- BẮT ĐẦU RESET DỮ LIỆU TOÀN DIỆN ---");
        
        // 1. Xóa file JSON
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
            Debug.Log("1a. Đã xóa file inventory.json");
        }
        if (File.Exists(TeamFilePath))
        {
            File.Delete(TeamFilePath);
            Debug.Log("1b. Đã xóa file team.json");
        }

        // 2. Xóa PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("2. Đã xóa sạch PlayerPrefs.");

        // 3. Khởi tạo lại dữ liệu mặc định ngay lập tức để tránh file trống
        Load(); 
        Debug.Log("3. Đã khởi tạo lại dữ liệu mặc định mới.");

        // 4. Đồng bộ hóa với GachaManager nếu nó đang tồn tại
        if (GachaManager.Instance != null)
        {
            GachaManager.Instance.RefreshData();
            Debug.Log("4. GachaManager đã cập nhật túi đồ mới.");
        }

        Debug.Log("--- RESET HOÀN TẤT ---");
    }

    public static TeamData LoadTeam()
    {
        TeamData team = new TeamData();
        if (File.Exists(TeamFilePath))
        {
            string json = File.ReadAllText(TeamFilePath);
            team = JsonUtility.FromJson<TeamData>(json);
        }

        // --- CHIẾN THUẬT TRIỆT ĐỂ (STRICT LOGIC) ---
        // Nếu đội hình ít hơn 5 con, bắt buộc phải lấp đầy cho đủ 5 (nếu túi đồ có đủ)
        if (team.selectedHeroIds.Count < 5)
        {
            PlayerInventory inv = Load(); // Gọi Load() ở đây cũng đã đảm bảo inv có đủ 5 starters
            foreach (string id in inv.ownedCharacterIds)
            {
                if (team.selectedHeroIds.Count >= 5) break;
                if (!team.selectedHeroIds.Contains(id))
                {
                    team.selectedHeroIds.Add(id);
                }
            }
            // Lưu lại ngay đội hình đã được sửa lỗi
            SaveTeam(team);
            Debug.Log($"InventorySystem: Đội hình thiếu tướng đã được tự động lấp đầy lên {team.selectedHeroIds.Count} con.");
        }

        return team;
    }

    public static void SaveTeam(TeamData teamData)
    {
        string json = JsonUtility.ToJson(teamData, true);
        File.WriteAllText(TeamFilePath, json);
    }
}
