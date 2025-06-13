using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffRoze : MonoBehaviour, ICardEffect
{
    private ScoreManager scoreManager;
    private GameMaster gameMaster;
    private CardManager cardManager;

    // 세미콜론(;) 제거, 타입 이름 대문자로 수정
    public void Init(ScoreManager scoreManager, GameMaster gameMaster, CardManager cardManager)
    {
        this.scoreManager = scoreManager;
        this.gameMaster = gameMaster;
        this.cardManager = cardManager;
        Debug.Log($"[BuffChuros] Init 호출됨! scoreManager: {(scoreManager != null)}, gameMaster: {(gameMaster != null)}, cardManager: {(cardManager != null)}");
    }

    public void Effect()
    {
        Debug.Log("로제 효과 발동!");

        if (scoreManager != null && cardManager != null)
        {
            // 적용 대상 카드 ID 목록
            int[] targetIds = { 5, 12, 19, 26, 33, 40, 47 };
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
            Debug.Log($"BuffChuros: {addCount}장에 대해 scoreYet +10 적용 (총 +{addCount * 10})");
        }
    }
}