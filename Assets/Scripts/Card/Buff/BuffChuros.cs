using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffChuros : MonoBehaviour, ICardEffect
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
        Debug.Log("churos ȿ�� �ߵ�!");
        if (scoreManager != null)
        {
            scoreManager.scoreYet += 100;
            Debug.Log($"���� ����: {scoreManager.score}");
        }
        // gameMaster Ȱ�뵵 ����
    }
}