using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardList", menuName = "GameData/CardList")]
public class CardListSO : ScriptableObject
{
    public List<GameObject> cards;
}
