using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBeldir : MonoBehaviour, ICardEffect
{
    public void Init(ScoreManager scoreManager, GameMaster gameMaster, CardManager cardManager)
    {
        // 필요하다면 매니저 참조를 저장해서 사용하세요.
        // 현재는 사용하지 않으면 비워둬도 무방합니다.
    }

    public void Effect()
    {
        Debug.Log("BasicBeldir 효과 발동!");
        // 이 카드만의 효과 구현
    }
}
