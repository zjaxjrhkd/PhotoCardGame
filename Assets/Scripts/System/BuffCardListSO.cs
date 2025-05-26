using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffCardListSO", menuName = "Card/BuffCardList")]
public class BuffCardListSO : ScriptableObject
{
    public List<GameObject> buffCards;
}
