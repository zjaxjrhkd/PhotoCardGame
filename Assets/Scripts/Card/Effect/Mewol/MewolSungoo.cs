using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MewolSungoo : MonoBehaviour, ICardEffect
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
        Debug.Log("sun");
        if (gameMaster != null)
        {
            if (gameMaster != null && gameMaster.musicManager != null)
                gameMaster.musicManager.PlayGetCoinSFX();
            gameMaster.PlusSetCount(); // ��Ʈ ����
        }
    }
}
