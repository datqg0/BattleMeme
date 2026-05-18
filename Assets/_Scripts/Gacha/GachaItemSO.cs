using UnityEngine;

[CreateAssetMenu(fileName = "New Gacha Item", menuName = "Gacha/Item")]
public class GachaItemSO : ScriptableObject
{
    public string id; // ID này phải khớp với ID trong file JSON
    public string characterName;
    public string rarity;
    public GameObject prefab; // Kéo thả prefab vào đây
    public Sprite icon;       // Icon hiển thị trong UI
    public UnitData unitData; // Thông số chiến đấu của tướng
}
