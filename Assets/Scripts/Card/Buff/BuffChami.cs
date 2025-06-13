using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffChami : MonoBehaviour, ICardEffect
{
    private ScoreManager scoreManager;
    private GameMaster gameMaster;
    private CardManager cardManager;

    // �����ݷ�(;) ����, Ÿ�� �̸� �빮�ڷ� ����
    public void Init(ScoreManager scoreManager, GameMaster gameMaster, CardManager cardManager)
    {
        this.scoreManager = scoreManager;
        this.gameMaster = gameMaster;
        this.cardManager = cardManager;
        Debug.Log($"[BuffChuros] Init ȣ���! scoreManager: {(scoreManager != null)}, gameMaster: {(gameMaster != null)}, cardManager: {(cardManager != null)}");
    }

    public void Effect()
    {
        Debug.Log("���� ȿ�� �ߵ�!");

        if (scoreManager != null && cardManager != null)
        {
            // ���� ��� ī�� ID ���
            int[] targetIds = { 4, 11, 18, 25, 32, 39, 46, 53 };
            int addCount = 0;

            foreach (var card in cardManager.checkCardList)
            {
                if (card == null) continue;
                CardData data = card.GetComponent<CardData>();
                if (data != null && System.Array.Exists(targetIds, id => id == data.cardId))
                {
                    scoreManager.scoreYet += 10;
                    addCount++;
                }
            }
            Debug.Log($"BuffChuros: {addCount}�忡 ���� scoreYet +10 ���� (�� +{addCount * 10})");
        }
    }
}
