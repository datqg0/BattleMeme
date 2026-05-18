using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "BatlleCats/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelNumber; // Số thứ tự màn chơi             

    [Header("Enemy Information")]
    public float enemyHealthMultiplier = 1f; 
    public float baseHealthMultiplier = 1f;  
    public List<UnitData> levelEnemies; // Danh sách các loại quái sẽ xuất hiện ở màn này
    public UnitData bossUnit;           // Tướng địch (Boss)
    public int enemiesToKillForBoss = 5; // Giết bao nhiêu quân địch thì Boss ra

    [Header("LV Imgage & Music")]
    public Sprite levelBackground; 
    public Sprite playerBaseSprite; // Ảnh nhà Ta
    public Sprite enemyBaseSprite;  // Ảnh nhà Địch
    public AudioClip levelBGM;      // Nhạc nền cho màn này

    [Header("Reward")]
    public int completionBonus;     // Thưởng khi thắng màn
    public int coinRewardPerEnemy;  // Tiền nhận được cho mỗi con quái bị diệt

    [Header("Next Level")]
    public LevelData nextLevel; // Kéo Level tiếp theo vào đây
}
