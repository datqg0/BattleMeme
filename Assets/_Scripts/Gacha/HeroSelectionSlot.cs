using UnityEngine;
using UnityEngine.UI;

public class HeroSelectionSlot : MonoBehaviour
{
    public Image icon;
    public Image background; // Hình nền của slot để đổi màu
    public Text nameText;
    public GameObject selectionVisual; // Hiện viền sáng hoặc dấu tích khi được chọn

    private string heroId;
    private HeroSelectionUI mainUI;
    public string HeroId => heroId;

    public void Setup(GachaItemSO item, HeroSelectionUI ui, bool isSelected)
    {
        heroId = item.id;
        mainUI = ui;
        
        if (icon != null) icon.sprite = item.icon; 
        
        if (nameText != null) nameText.text = item.characterName;
        
        SetSelected(isSelected);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionVisual != null) selectionVisual.SetActive(isSelected);
        
        // Đổi màu nền để nhận biết rõ ràng hơn
        if (background != null) background.color = isSelected ? new Color(0.7f, 1f, 0.7f) : Color.white;

        // Hiệu ứng scale nhỏ để tạo cảm giác tương tác
        transform.localScale = isSelected ? Vector3.one * 1.05f : Vector3.one;
    }

    // Hàm này sẽ được gán vào Button component trong Prefab
    public void OnClick()
    {
        if (mainUI != null)
        {
            mainUI.SetTemporarySelection(heroId);
        }
    }
}
