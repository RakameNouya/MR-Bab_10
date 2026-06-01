using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public InventoryManager inventory;
    public GameObject iconPrefab; // Prefab Image sederhana untuk icon item
    public Transform container;   // Tempat icon-icon akan muncul (Grid Layout)

    public void UpdateInventoryDisplay()
    {
        // Hapus tampilan lama
        foreach (Transform child in container) { Destroy(child.gameObject); }

        // Tampilkan yang baru
        foreach (TreasureItem item in inventory.collectedItems)
        {
            GameObject newIcon = Instantiate(iconPrefab, container);
            newIcon.GetComponent<Image>().sprite = item.itemIcon;
        }
    }
}


