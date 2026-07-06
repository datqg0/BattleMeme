using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
public class LightningBuff : MonoBehaviour
{
    public float lightningDamage = 300f;

    public float cooldownDuration = 30f;

    [Header("Animation / Hieu ung")]
    public GameObject lightningEffects;

    public AudioClip lightningSFX;

    [Header("UI Button")]
    public Button lightningButton;
    [Tooltip("Kéo một Image có Image Type là Filled vào đây để làm hiệu ứng vòng quay")]
    public Image cooldownFillImage;

    private float cooldownRemaining = 0f;
    private bool isOnCooldown = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        SetButtonInteractable(true);

        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = 0f;
        }
        
        if (lightningEffects != null)
        {
            lightningEffects.SetActive(false);
        }
    }

    void Update()
    {
        if (!isOnCooldown) return;

        cooldownRemaining -= Time.deltaTime;

        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = cooldownRemaining / cooldownDuration;
        }

        if (cooldownRemaining <= 0f)
        {
            cooldownRemaining = 0f;
            isOnCooldown = false;
            SetButtonInteractable(true);
        }
    }

    // Ham goi tu Button OnClick
    public void ActivateLightning()
    {
        if (isOnCooldown) return;

        isOnCooldown = true;
        cooldownRemaining = cooldownDuration;
        SetButtonInteractable(false);

        StartCoroutine(LightningSequence());
    }

    IEnumerator LightningSequence()
    {
        // 1. Phat am thanh
        if (lightningSFX != null && audioSource != null)
            audioSource.PlayOneShot(lightningSFX);

        // 2. Bat (Enable) cac game object hieu ung set co san
        if (lightningEffects != null)
        {
            lightningEffects.SetActive(true);
        }

        // 3. Doi animation den doan dinh cao roi moi deal dmg
        yield return new WaitForSeconds(1f);

        // 4. Gay sat thuong CHI CHO KE DICH
        DealDamageToEnemiesOnly();

        // 5. Doi animation ket thuc roi Tat (Disable) hieu ung
        float remainingAnimTime = 1.2f - 0.3f; // Tong thoi gian la 1.2s, tru di 0.3s da cho
        yield return new WaitForSeconds(remainingAnimTime);

        if (lightningEffects != null)
        {
            lightningEffects.SetActive(false);
        }
    }

    void DealDamageToEnemiesOnly()
    {
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        int count = 0;
        foreach (Unit unit in allUnits)
        {
            if (!unit.isPlayerUnit)
            {
                unit.TakeDamage(lightningDamage);
                count++;
            }
        }

        Debug.Log("<color=yellow>Set Danh! Da gay " + lightningDamage + " sat thuong cho " + count + " ke dich.</color>");
    }

    void SetButtonInteractable(bool interactable)
    {
        if (lightningButton != null)
        {
            lightningButton.interactable = interactable;

            // Xử lý đè màu Disabled của hệ thống UI Button
            ColorBlock cb = lightningButton.colors;
            cb.disabledColor = Color.red;
            lightningButton.colors = cb;

            if (lightningButton.image != null)
            {
                lightningButton.image.color = interactable ? Color.white : Color.red;
            }
        }
    }
}
