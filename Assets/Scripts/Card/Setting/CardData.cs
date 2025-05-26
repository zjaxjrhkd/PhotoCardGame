using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData : MonoBehaviour
{
    public int cardId;
    public string cardName;

    public enum CardType { Character, Buff }
    public CardType cardType;
}
