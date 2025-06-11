using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffChami : MonoBehaviour
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

        Debug.Log("차미 효과 발동!");
        if (gameMaster != null)
        {
            gameMaster.maxDropCount += 1;
            Debug.Log($"Drop 증가: {gameMaster.maxDropCount}");
        }
        // gameMaster 활용도 가능
    }
}
