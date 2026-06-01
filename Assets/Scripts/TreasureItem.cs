using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "TreasureHunt/Item")]
public class TreasureItem : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public string question;
    public string[] answers;
    public int correctAnswerIndex;
}