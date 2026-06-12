using UnityEngine;
using UnityEngine.UI; // Để dùng Slider

public class Unit : MonoBehaviour
{
    [Header("Dữ liệu cấu hình")]
    public UnitData data;           // Kéo file ScriptableObject vào đây
    public bool isPlayerUnit;       // Tick vào nếu là lính của Ta, không tick là Địch

    [Header("Giao diện & Hiệu ứng")]
    public GameObject hpSliderObject; // Kéo Object Slider vào đây (không lo lỗi Type Mismatch nữa)
    private Slider hpSlider;         // Biến này sẽ tự tìm Slider bên trong Object trên
    private Animator anim;          // Biến điều khiển Animation (ĐÃ THÊM LẠI)

    private float currentHealth;    
    public float healthBonus = 1f;  // Hệ số nhân thêm (dành cho Boss)
    public bool isBoss;             // Đánh dấu nếu con này là Boss
    private float nextAttackTime;   
    private Rigidbody2D rb;
    private SpriteRenderer sr;   // Cache lại để không gọi GetComponent mỗi frame
    private bool isDead = false;
    public GameObject deadEffectPrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>(); // Cache một lần duy nhất

        // Tự động tìm linh kiện Slider từ Object bạn kéo vào
        if (hpSliderObject != null)
        {
            hpSlider = hpSliderObject.GetComponent<Slider>();
        }

        if (data != null)
        {
            currentHealth = data.health * healthBonus;

            // ĐỔI BỘ ANIMATION RIÊNG CHO TỪNG LOẠI LÍNH (Nếu có)
            if (anim != null && data.unitAnimator != null)
            {
                anim.runtimeAnimatorController = data.unitAnimator;
            }

            // ÁP DỤNG KÍCH THƯỚC (TO/NHỎ)
            transform.localScale *= data.visualScale;

            // ÁP DỤNG ĐỘ LỆCH VỊ TRÍ
            transform.position += data.spawnOffset;

            // 1. THIẾT LẬP MÁU VÀ THANH MÁU
            if (!isPlayerUnit && GameManager.Instance != null)
            {
                currentHealth *= GameManager.Instance.currentLevel.enemyHealthMultiplier;
            }

            if (hpSlider != null)
            {
                hpSlider.maxValue = currentHealth;
                hpSlider.value = currentHealth;

                // ĐỔI MÀU THANH MÁU CHO ĐỊCH
                if (!isPlayerUnit)
                {
                    Image fillImage = hpSlider.fillRect.GetComponent<Image>();
                    if (fillImage != null) fillImage.color = Color.red;
                }
            }

            // 2. ĐỔI HÌNH ẢNH (Dùng cached sr)
            if (sr != null) sr.sprite = data.unitSprite;

            // 3. ĐỔI BỘ ANIMATION RIÊNG
            if (anim != null)
            {
                if (data.unitAnimator != null)
                {
                    anim.runtimeAnimatorController = data.unitAnimator;
                    anim.enabled = true;
                }
                else
                {
                    // Nếu chưa có Animation riêng, tạm tắt Animator để nó không đè hình gốc
                    anim.enabled = false; 
                }
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        // ÉP BUỘC PHE DỰA VÀO TAG (Phải làm liên tục để đảm bảo không sai sót)
        if (gameObject.CompareTag("Player")) 
            isPlayerUnit = true;
        else if (gameObject.CompareTag("Enemy")) 
            isPlayerUnit = false;

        // 1. Kiểm tra mục tiêu
        GameObject target = CheckForTarget();
        float direction = isPlayerUnit ? 1 : -1;

        if (target != null)
        {
            // Nếu lỡ chạm vào nhà mình thì bỏ qua, đi tiếp
            BaseHealth b = target.GetComponent<BaseHealth>();
            if (b != null && b.isPlayerBase == isPlayerUnit)
            {
                Move(direction);
                return;
            }

            // Dừng lại đánh
            rb.linearVelocity = Vector2.zero; 
            SafeSetAnimBool("isAttacking", true);

            if (Time.time >= nextAttackTime)
            {
                Attack(target);
                nextAttackTime = Time.time + data.attackInterval;
            }
        }
        else
        {
            Move(direction);
        }

        // 2. CẬP NHẬT SORTING ORDER DỰA TRÊN CHÂN SPRITE (Tạo chiều sâu chính xác)
        if (sr != null)
        {
            // Dùng bounds.min.y (chân sprite) thay vì tâm → Boss to vẫn sort đúng
            float footY = sr.bounds.min.y;
            sr.sortingOrder = Mathf.RoundToInt(footY * -10);
        }

        // Vẽ vùng kiểm tra (để Debug)
        Vector2 boxSize = new Vector2(0.1f, 0.5f); // Quét một vùng cao 0.5 để bắt được lính lệch Y
        Debug.DrawRay(transform.position + (Vector3.right * direction * 0.2f), Vector2.right * direction * data.attackRange, Color.red);
    }

    void Move(float direction)
    {
        SafeSetAnimBool("isAttacking", false);
        rb.linearVelocity = new Vector2(direction * data.moveSpeed, rb.linearVelocity.y);

        if (sr != null) sr.flipX = (direction < 0);
    }

    // HÀM AN TOÀN: Chỉ bật animation nếu trong bộ Animator có tên đó
    void SafeSetAnimBool(string paramName, bool value)
    {
        if (anim != null && anim.enabled && anim.runtimeAnimatorController != null)
        {
            // Kiểm tra xem tham số có tồn tại không để tránh báo lỗi vàng
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == paramName)
                {
                    anim.SetBool(paramName, value);
                    return;
                }
            }
        }
    }

    GameObject CheckForTarget()
    {
        float direction = isPlayerUnit ? 1 : -1;
        LayerMask targetLayer = isPlayerUnit ? LayerMask.GetMask("Enemy") : LayerMask.GetMask("Player");
        
        Vector2 rayOrigin = (Vector2)transform.position + (Vector2.right * direction * 0.3f);

        // DÙNG BOXCAST ĐỂ QUÉT MỘT VÙNG RỘNG HƠN
        // Nếu là Boss (to hơn), ta quét một vùng CỰC CAO để không bị bắn trượt qua đầu lính nhỏ
        float scanHeight = isBoss ? 3.0f : 0.8f; 
        Vector2 boxSize = new Vector2(0.1f, scanHeight); 
        
        RaycastHit2D hit = Physics2D.BoxCast(rayOrigin, boxSize, 0f, Vector2.right * direction, data.attackRange, targetLayer);

        if (hit.collider != null)
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    void Attack(GameObject target)
    {
        Unit targetUnit = target.GetComponent<Unit>();
        if (targetUnit != null)
        {
            // BẢO VỆ: Nếu mục tiêu cùng phe (cùng là Player hoặc cùng là Enemy) thì không đánh
            if (targetUnit.isPlayerUnit == this.isPlayerUnit) return;

            targetUnit.TakeDamage(data.attackDamage);
            return;
        }

        BaseHealth targetBase = target.GetComponent<BaseHealth>();
        if (targetBase != null)
        {
            // BẢO VỆ: Lính Ta không đánh Nhà Ta, lính Địch không đánh Nhà Địch
            if (targetBase.isPlayerBase == this.isPlayerUnit) return;

            targetBase.TakeDamage(data.attackDamage);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Cập nhật thanh máu ngay lập tức
        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }


    void Die()
    {
        isDead = true;

        // Nếu là địch bị tiêu diệt
        if (!isPlayerUnit && GameManager.Instance != null && data != null)
        {
            // GameManager.Instance.AddCoins(data.rewardCoins); // Dòng cũ
            if(deadEffectPrefab != null) Instantiate(deadEffectPrefab, new Vector3(transform.position.x, transform.position.y, -0.1f), Quaternion.identity);
            GameManager.Instance.RegisterEnemyKilled(isBoss); // Truyền thêm biến isBoss vào đây
        }
        
        // Nếu là lính Ta bị tiêu diệt
        if (isPlayerUnit && GameManager.Instance != null)
        {
            if(deadEffectPrefab != null) Instantiate(deadEffectPrefab, new Vector3(transform.position.x, transform.position.y, -0.1f), Quaternion.identity);
            GameManager.Instance.DecreasePlayerCount();
        }

        Destroy(gameObject);
    }
}