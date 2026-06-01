using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<TreasureItem> collectedItems = new List<TreasureItem>();

    public void AddItem(TreasureItem item)
    {
        collectedItems.Add(item);
        Debug.Log("Berhasil mengambil: " + item.itemName);
        // Di sini kamu bisa memicu animasi "Item Obtained!"
    }
}
