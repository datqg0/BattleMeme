using UnityEngine;


[CreateAssetMenu(fileName = "NewUnitData", menuName = "BattleCats/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Basic Information")]
    public string unitName;
    public int cost;
    public int rewardCoins; // Xu nhận được khi tiêu diệt lính này (Tách riêng với Tiền trong trận)
    public Sprite unitSprite;
    public RuntimeAnimatorController unitAnimator; // Kéo bộ Animator của riêng con lính này vào đây
    public float visualScale = 1f; // Tỉ lệ kích thước (1 là bình thường, >1 là to hơn)
    public Vector3 spawnOffset = Vector3.zero; // Độ lệch vị trí khi sinh ra (Mặc định là 0,0,0)

    [Header("Combat Index")]
    public float health;
    public float attackDamage;
    public float attackRange;
    public float attackInterval;
    public float moveSpeed;

    [Header("Time cooldown")]
    public float spawnCooldown; 

    [Header("Boss Settings")]
    public AudioClip bossMusic; // Nhạc nền riêng khi con này làm Boss
}
