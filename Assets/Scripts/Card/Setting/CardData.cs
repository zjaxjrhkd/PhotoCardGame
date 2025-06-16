using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData : MonoBehaviour
{
    public int cardId;
    public string cardName;

    public enum CardType { Character, Buff }
    public CardType cardType;

    public enum BuffType { None, Always, OnStart } // Ãß°¡
    public BuffType buffType;

    public enum PlayType { Hand, Check }
    public PlayType playType;

    public enum CharacterType { Jooin, Beldir, Iana, Sitry, Noi, Limi, Raz, Churos, Chami, Donddatge, Muddung, Rasky, Roze, Yasu }
    public CharacterType characterType;

    public void InitEffects(ScoreManager scoreManager, GameMaster gameMaster, CardManager cardManager)
    {
        var effects = GetComponents<ICardEffect>();
        foreach (var effect in effects)
        {
            effect.Init(scoreManager, gameMaster, cardManager);
        }
    }

    public void UseEffect()
    {
        var effects = GetComponents<ICardEffect>();
        foreach (var effect in effects)
        {
            effect.Effect();
        }
    }
}
