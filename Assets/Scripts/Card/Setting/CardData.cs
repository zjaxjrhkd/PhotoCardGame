using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // ���� ��ܿ� �߰�

public class CardData : MonoBehaviour
{
    public int cardId;
    public string cardName;

    public enum CardType { Character, Buff }
    public CardType cardType;

    public enum BuffType { None, Always, OnStart } // �߰�
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
        // ī�� ��鸲 �ִϸ��̼� (Y������ 0.5��ŭ 3ȸ)
        Transform tr = this.transform;
        float duration = 0.12f; // �� �� �պ� �ð�
        int loopCount = 3;

        // ���� ��ġ ����
        Vector3 origin = tr.position;

        // DOTween ������
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < loopCount; i++)
        {
            seq.Append(tr.DOMoveY(origin.y + 0.5f, duration).SetEase(Ease.OutQuad));
            seq.Append(tr.DOMoveY(origin.y, duration).SetEase(Ease.InQuad));
        }
        seq.Play();

        // ���� ȿ�� ����
        var effects = GetComponents<ICardEffect>();
        foreach (var effect in effects)
        {
            effect.Effect();
        }
    }
}
