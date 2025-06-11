using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBeldir : MonoBehaviour, ICardEffect
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
        Debug.Log("BasicBeldir 효과 발동!");
        if (gameMaster != null)
        {
            gameMaster.coin ++; // 코인 증가
        }
    }
}
