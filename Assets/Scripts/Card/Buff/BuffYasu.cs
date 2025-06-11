using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffYasu : MonoBehaviour, ICardEffect
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
        Debug.Log("YASU ȿ�� �ߵ�!");
        if (scoreManager != null)
        {
            scoreManager.rate += 0.5f;
            Debug.Log($"rate ����: {scoreManager.rate}");
        }
        // gameMaster Ȱ�뵵 ����
    }
}