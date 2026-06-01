using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizClick : MonoBehaviour
{
    // Masukkan objek Cube (yang ada script TreasureClick-nya) ke sini
    public GameObject TreasureItems;

    private void OnMouseDown()
    {
        Debug.Log("Quiz Selesai! Harta muncul.");

        if (TreasureItems != null)
        {
            TreasureItems.SetActive(true); // Memunculkan harta karun
        }

        // Hilangkan tombol quiz setelah diklik agar tidak bisa di-spam
        gameObject.SetActive(false);
    }
}