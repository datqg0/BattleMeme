using System.Collections.Generic;
using TMPro;

using UnityEngine;
public class HeroSelectionManager : MonoBehaviour
{
    private static HeroSelectionManager _instance;
    private TextMeshProUGUI StoreInformationText;
    public static HeroSelectionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<HeroSelectionManager>();
                if (_instance == null)
                {
                    Debug.LogWarning("HeroSelectionManager not found in scene. Creating a new instance.");
                    GameObject go = new GameObject("HeroSelectionManager");
                    _instance = go.AddComponent<HeroSelectionManager>();
                }
            }
            return _instance;
        }
    }

    public List<GachaItemSO> itemDatabase;
    private GameObject currentSpawnedHero;
    private Transform spawnPoint;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    public GameObject GetPrefabById(string id)
    {
        var item = itemDatabase.Find(x => x.id == id);
        if(item!=null) {
            StoreInformationText.text = item.UnitStory;
        }
        return item != null ? item.prefab : null;
    }

    public void Setup (List<GachaItemSO> item,Transform Spawnpos,TextMeshProUGUI Story)
    {
        itemDatabase = item;
        spawnPoint = Spawnpos;
        StoreInformationText = Story;
    }
    public void DisplayHero(string item)
        {
            if (itemDatabase == null)
            {
                Debug.LogError("HeroSelectionManager.itemDatabase is null! Call Setup() first.");
                return;
            }
            
            if (spawnPoint == null)
            {
                Debug.LogError("HeroSelectionManager.spawnPoint is null! Call Setup() first.");
                return;
            }
            
            if (currentSpawnedHero != null) Destroy(currentSpawnedHero);
            GameObject prefab = GetPrefabById(item);
                
            if (prefab != null)
            {
                currentSpawnedHero = Instantiate(prefab, spawnPoint);
                    
                // NGĂN DI CHUYỂN: Tắt các script điều khiển và vật lý
                Unit unitScript = currentSpawnedHero.GetComponent<Unit>();
                if (unitScript != null) {
                    unitScript.enabled = false;
                    // Tắt thanh máu cho đẹp
                    if (unitScript.hpSliderObject != null) unitScript.hpSliderObject.SetActive(false);
                }
                Rigidbody2D rb = currentSpawnedHero.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = false;
                // Reset vị trí (đẩy Z về phía trước để hiện trên UI) và scale
                currentSpawnedHero.transform.localPosition = new Vector3(0, 0, -10f);
                currentSpawnedHero.transform.localScale = Vector3.one * 80f; 
                // Nếu là game 2D, đảm bảo Sorting Order cao để hiện lên trên
                SpriteRenderer[] srs = currentSpawnedHero.GetComponentsInChildren<SpriteRenderer>();
                foreach(var sr in srs) {
                    sr.sortingOrder = 100; 
                }
                Debug.Log("Spawned");
            }
            else {
                Debug.Log("null prefab");
            }
        }
}