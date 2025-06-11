using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffYasu : MonoBehaviour, ICardEffect
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
        Debug.Log("YASU 효과 발동!");
        if (scoreManager != null)
        {
            scoreManager.rate += 0.5f;
            Debug.Log($"rate 증가: {scoreManager.rate}");
        }
        // gameMaster 활용도 가능
    }
}