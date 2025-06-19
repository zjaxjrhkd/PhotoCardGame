using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSitry : MonoBehaviour, ICardEffect
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
    }
    public void Effect()
    {
        Debug.Log("��Ʈ�� ȿ�� �ߵ�!");
        if (scoreManager != null && gameMaster != null && gameMaster.stageManager != null)
        {
            int stageIndex = gameMaster.stageManager.GetCurrentStageIndex();
            scoreManager.scoreYet += stageIndex * 10;
            Debug.Log($"��Ʈ�� ȿ��: ���� �������� �ε���({stageIndex}) x 10 = {stageIndex * 10} �� �߰�!");
        }
    }
}
