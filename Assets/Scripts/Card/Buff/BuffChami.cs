using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffChami : MonoBehaviour
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
        if (gameMaster != null)
        {
            gameMaster.maxDropCount += 1;
            Debug.Log($"Drop ����: {gameMaster.maxDropCount}");
        }
        // gameMaster Ȱ�뵵 ����
    }
}
