using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    public float scaleFactor = 0.9f; // Thu nhỏ lại 10% khi bấm
    public float animationSpeed = 0.1f;
    public bool useSound = true;
    public AudioClip clickSound;

    private Vector3 originalScale;
    private Coroutine currentCoroutine;
    private AudioSource audioSource;

    void Awake()
    {
        originalScale = transform.localScale;
        
        // Tự động thêm AudioSource nếu cần
        if (useSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    // Khi rê chuột vào
    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAndStartLerp(originalScale * 1.05f); // Phóng to nhẹ 5%
    }

    // Khi rê chuột ra
    public void OnPointerExit(PointerEventData eventData)
    {
        StopAndStartLerp(originalScale);
    }

    // Khi nhấn chuột xuống
    public void OnPointerDown(PointerEventData eventData)
    {
        StopAndStartLerp(originalScale * scaleFactor);
        
        if (useSound && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // Khi thả chuột ra
    public void OnPointerUp(PointerEventData eventData)
    {
        StopAndStartLerp(originalScale * 1.05f);
    }

    private void StopAndStartLerp(Vector3 targetScale)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(LerpScale(targetScale));
    }

    IEnumerator LerpScale(Vector3 target)
    {
        float timer = 0;
        Vector3 startScale = transform.localScale;

        while (timer < animationSpeed)
        {
            transform.localScale = Vector3.Lerp(startScale, target, timer / animationSpeed);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = target;
    }

    void OnDisable()
    {
        // Reset scale khi object bị tắt
        transform.localScale = originalScale;
    }
}
