using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicJooin : MonoBehaviour, ICardEffect
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
    }

    public void Effect()
    {
        Debug.Log("BasicJooin 효과 발동!");
        if (scoreManager != null)
        {
            scoreManager.score += 100;
        }
        // gameMaster 활용도 가능
    }
}
